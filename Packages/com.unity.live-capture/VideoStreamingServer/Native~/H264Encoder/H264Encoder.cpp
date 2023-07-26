#include "stdafx.h"

#define USE_TEST_CONTENT 0
#define USE_MONOCHROME_CONTENT 0
#define ENABLE_TRACE 0

#if ENABLE_TRACE
#include <codecvt>
#include <chrono>
#include <fstream>
#include <locale>
#include <mutex>
#include <sstream>

static std::ofstream g_logFile;
#define TRACE_HEX(val) std::hex << std::uppercase << val << std::nouppercase << std::dec
#define TRACE_TIMESTAMP (std::chrono::duration_cast<std::chrono::microseconds>(std::chrono::time_point_cast<std::chrono::microseconds>(std::chrono::high_resolution_clock::now()).time_since_epoch()).count())
#define TRACE(msg) { std::stringstream os; os << TRACE_TIMESTAMP << " | " << msg << std::endl; g_logFile << os.str(); g_logFile.flush(); }
#else
#define TRACE(msg) {}
#endif

#include <array>
#include <codecapi.h>
#include <comdef.h>
#include <mfapi.h>
#include <mferror.h>
#include <mfidl.h>
#include <mfobjects.h>
#include <mftransform.h>
#include <vector>
#include <wmcodecdsp.h>

#pragma comment(lib, "mfplat.lib")
#pragma comment(lib, "mfuuid.lib")
#pragma comment(lib, "wmcodecdspuuid.lib")

_COM_SMARTPTR_TYPEDEF(ICodecAPI, IID_ICodecAPI);
_COM_SMARTPTR_TYPEDEF(IMFAttributes, IID_IMFAttributes);
_COM_SMARTPTR_TYPEDEF(IMFMediaType, IID_IMFMediaType);
_COM_SMARTPTR_TYPEDEF(IMFMediaBuffer, IID_IMFMediaBuffer);
_COM_SMARTPTR_TYPEDEF(IMFSample, IID_IMFSample);
_COM_SMARTPTR_TYPEDEF(IMFTransform, IID_IMFTransform);

#define CHECK_HR_RET(hrSrc, msg) \
{ \
    const HRESULT hr = hrSrc; \
	if (hr != S_OK) \
	{ \
        TRACE(msg << ". Error: " << TRACE_HEX(hr)); \
		return false; \
	} \
}

// Media Foundation H.264 encoder uses the 4-bytes Annex B format (not AVCC), so each NALU is
// prefixed with 0x00000001.
enum
{
	kAnnexBPrefixSize = 4,
};

static void FindHardwareEncoder(IMFTransformPtr& decoder)
{
	MFT_REGISTER_TYPE_INFO info = {};
	info.guidMajorType = MFMediaType_Video;
	info.guidSubtype = MFVideoFormat_H264;
	IMFActivate **ppActivate = NULL;
	UINT32 count = 0;
	HRESULT hr = MFTEnumEx(
		MFT_CATEGORY_VIDEO_ENCODER,
		MFT_ENUM_FLAG_HARDWARE | MFT_ENUM_FLAG_SYNCMFT | MFT_ENUM_FLAG_ASYNCMFT | MFT_ENUM_FLAG_LOCALMFT | MFT_ENUM_FLAG_SORTANDFILTER,
		&info,      // Input type
		NULL,       // Output type
		&ppActivate,
		&count);

	if (SUCCEEDED(hr) && count == 0)
		return;

	TRACE("H264Encoder::FindHardwareEncoder found " << count << " encoders. Taking first.");

	// Create the first decoder in the list.
	if (SUCCEEDED(hr))
		ppActivate[0]->ActivateObject(IID_PPV_ARGS(&decoder));

	for (UINT32 i = 0; i < count; ++i)
	{
#if ENABLE_TRACE
		WCHAR name[1024];
		UINT32 nameLength = 0;
		HRESULT hr = ppActivate[i]->GetString(MFT_FRIENDLY_NAME_Attribute, name, 1024, &nameLength);
		if (hr != S_OK || nameLength == 0)
		{
			TRACE("Encoder " << i << ": <unknown>");
		}
		else
		{
			std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>, wchar_t> conv;
			std::wstring nameWStr(name);
			std::string nameStr = conv.to_bytes(nameWStr);
			TRACE("Encoder " << i << ": " << nameStr);
		}
#endif
		ppActivate[i]->Release();
	}
	CoTaskMemFree(ppActivate);
}

class H264Encoder 
{
public:

    H264Encoder()
		: m_OutputData{ 0, nullptr, 0, nullptr }
    {
		TRACE("H264Encoder::H264Encoder");
	}

    ~H264Encoder()
    {
		TRACE("H264Encoder::~H264Encoder");
        Stop();
    }

    void Stop()
    {
		TRACE("H264Encoder::Stop")
	}

	bool Initialize(
        const uint32_t width,
        const uint32_t height,
        const uint32_t frameRateNumerator,
        const uint32_t frameRateDenominator,
        const uint32_t averageBitRate,
        const uint32_t gopSize)
	{
		TRACE("H264Encoder::Initialize " << width << " x " << height << " @" << frameRateNumerator << "/" << frameRateDenominator << "fps, " << averageBitRate << " bps");

		// Create H.264 encoder.
		FindHardwareEncoder(m_Transform);
		if (!m_Transform)
		{
			TRACE("H264Encoder::Initialize: Could not find hardware encoder, using default.");
			IUnknownPtr transformUnk;
			CHECK_HR_RET(CoCreateInstance(CLSID_CMSH264EncoderMFT, nullptr, CLSCTX_INPROC_SERVER,
				IID_IUnknown, (void**)&transformUnk.GetInterfacePtr()), "Failed to create H264 encoder MFT");

			CHECK_HR_RET(transformUnk->QueryInterface(&m_Transform),
				"Failed to get IMFTransform interface from H264 encoder MFT object");
		}

		CHECK_HR_RET(m_Transform->QueryInterface(&m_Codec), "Failed to get ICodecAPI for transform");

		IMFAttributesPtr transformAttributes;
		m_Transform->GetAttributes(&transformAttributes);

		if (transformAttributes)
		{
			UINT32 isAsync = 0;
			HRESULT hr = transformAttributes->GetUINT32(MF_TRANSFORM_ASYNC, &isAsync);
			m_IsTransformAsync = hr == S_OK && (isAsync != 0);

			UINT32 hwUrlLength = 0;
			hr = transformAttributes->GetStringLength(MFT_ENUM_HARDWARE_URL_Attribute, &hwUrlLength);
			m_IsTransformHardware = hr == S_OK && hwUrlLength > 0;

			if (transformAttributes->SetUINT32(CODECAPI_AVLowLatencyMode, TRUE) == S_OK)
				TRACE("Set low latency mode succeeded.")
			else
				TRACE("Set low latency mode failed.")
		}

		// Create a media type describing the compressed data that is expected from the transform.
		IMFMediaTypePtr mftOutputMediaType;
		MFCreateMediaType(&mftOutputMediaType);
		mftOutputMediaType->SetGUID(MF_MT_MAJOR_TYPE, MFMediaType_Video);
		mftOutputMediaType->SetGUID(MF_MT_SUBTYPE, MFVideoFormat_H264);
		VARIANT var = { 0 };

        var.vt = VT_UI4;
        var.lVal = gopSize;
        CHECK_HR_RET(m_Codec->SetValue(&CODECAPI_AVEncMPVGOPSize, &var), "Failed to set GOP size");

        CHECK_HR_RET(mftOutputMediaType->SetUINT32(MF_MT_MPEG2_PROFILE, eAVEncH264VProfile_ConstrainedBase),
            "Failed to set profile on H264 MFT out type");
        CHECK_HR_RET(mftOutputMediaType->SetUINT32(CODECAPI_AVEncCommonRateControlMode, eAVEncCommonRateControlMode_LowDelayVBR),
            "Failed to set rate control mode on H264 output media type");
		// FIXME: Overwriting incoming value with estimation based on tmedia_get_video_bandwidth_kbps_2 as used in
		// https://github.com/noahseis/webrtc2sip/blob/master/doubango-source/branches/2.0/doubango/plugins/pluginWinMF/plugin_win_mf_producer_video.cxx
		//averageBitRate = static_cast<uint32_t>(0.14 * width * height * frameRateNumerator / frameRateDenominator);
        CHECK_HR_RET(mftOutputMediaType->SetUINT32(MF_MT_AVG_BITRATE, averageBitRate),
			"Failed to set average bit rate on H264 output media type");
		CHECK_HR_RET(MFSetAttributeSize(mftOutputMediaType, MF_MT_FRAME_SIZE, width, height),
			"Failed to set frame size on H264 MFT out type");
		CHECK_HR_RET(MFSetAttributeRatio(mftOutputMediaType, MF_MT_FRAME_RATE, frameRateNumerator, frameRateDenominator), 
			"Failed to set frame rate on H264 MFT out type");
		CHECK_HR_RET(MFSetAttributeRatio(mftOutputMediaType, MF_MT_PIXEL_ASPECT_RATIO, 1, 1),
			"Failed to set aspect ratio on H264 MFT out type");
        CHECK_HR_RET(mftOutputMediaType->SetUINT32(MF_MT_INTERLACE_MODE, 2), // 2 = Progressive scan, i.e. non-interlaced. 
            "Failed to set interlace mode to 2");
		CHECK_HR_RET(m_Transform->SetOutputType(0, mftOutputMediaType, 0), 
			"Failed to set output media type on H.264 encoder MFT");

		IMFMediaTypePtr mftInputMediaType;
		MFCreateMediaType(&mftInputMediaType);
		mftInputMediaType->SetGUID(MF_MT_MAJOR_TYPE, MFMediaType_Video);

		// Using NV12 format, assuming it has higher performance.
		mftInputMediaType->SetGUID(MF_MT_SUBTYPE, MFVideoFormat_NV12);

		CHECK_HR_RET(MFSetAttributeSize(mftInputMediaType, MF_MT_FRAME_SIZE, width, height),
			"Failed to set frame size on H264 MFT out type");
		CHECK_HR_RET(MFSetAttributeRatio(mftInputMediaType, MF_MT_FRAME_RATE, frameRateNumerator, frameRateDenominator), 
			"Failed to set frame rate on H264 MFT out type");
		CHECK_HR_RET(MFSetAttributeRatio(mftInputMediaType, MF_MT_PIXEL_ASPECT_RATIO, 1, 1), 
			"Failed to set aspect ratio on H264 MFT out type");
		CHECK_HR_RET(mftInputMediaType->SetUINT32(MF_MT_INTERLACE_MODE, 2),
			"Failed to set interlace mode to 2");
		CHECK_HR_RET(m_Transform->SetInputType(0, mftInputMediaType, 0), 
			"Failed to set input media type on H.264 encoder MFT");

		DWORD mftStatus = 0;
		CHECK_HR_RET(m_Transform->GetInputStatus(0, &mftStatus), 
			"Failed to get input status from H.264 MFT");
		if (MFT_INPUT_STATUS_ACCEPT_DATA != mftStatus)
		{
			TRACE("H.264 MFT not accepting data.");
			return false;
		}

		CHECK_HR_RET(m_Transform->ProcessMessage(MFT_MESSAGE_NOTIFY_BEGIN_STREAMING, NULL),
			"Failed to process BEGIN_STREAMING command on H.264 MFT");
		CHECK_HR_RET(m_Transform->ProcessMessage(MFT_MESSAGE_NOTIFY_START_OF_STREAM, NULL),
			"Failed to process START_OF_STREAM command on H.264 MFT");

		m_OutputData = {};

		MFT_OUTPUT_STREAM_INFO outputStreamInfo = {};
		CHECK_HR_RET(m_Transform->GetOutputStreamInfo(0, &outputStreamInfo), "Failed to get output stream info from H264 MFT.\n");

		if (!ParseSpsPps(mftOutputMediaType))
			return false;

        m_FrameRateNumerator = frameRateNumerator;
		m_FrameRateDenominator = frameRateDenominator;
		m_Width = width;
		m_Height = height;
#if USE_TEST_CONTENT
		// Create Y and interleaved U/V that are expected by NV12.
		const int pixCount = width * height;
		m_TempImage.resize(pixCount * 3 / 2);

		int idx = 0;
		for (int i = 0; i < pixCount; ++i, ++idx)
		{
			m_TempImage[idx] = 127;
		}
		const int halfPixCount = pixCount / 2;

		for (int i = 0; i < halfPixCount; i += 2, idx += 2)
		{
			m_TempImage[idx] = 200;
			m_TempImage[idx + 1] = 20;
		}
#endif

#if ENABLE_TRACE

		VARIANT val = {};
		if (m_Codec->GetValue(&CODECAPI_AVEncMPVGOPSize, &val) == S_OK)
			TRACE("AVEncMPVGOPSize: " << val.uintVal);
		if (m_Codec->GetValue(&CODECAPI_AVLowLatencyMode, &val) == S_OK)
			TRACE("AVLowLatencyMode: " << val.ulVal);
		if (m_Codec->GetValue(&CODECAPI_AVEncNumWorkerThreads, &val) == S_OK)
			TRACE("AVEncNumWorkerThreads: " << val.ulVal);
		
		val = {};

#endif
		return true;
	}

    uint32_t GetSps(uint8_t* const spsOut)
    {
		if (spsOut != nullptr)
			memcpy(spsOut, m_Sps.data(), m_Sps.size());
        return static_cast<uint32_t>(m_Sps.size());
    }

	uint32_t GetPps(uint8_t* const ppsOut)
	{
		if (ppsOut != nullptr)
			memcpy(ppsOut, m_Pps.data(), m_Pps.size());
		return static_cast<uint32_t>(m_Pps.size());
	}

	bool Encode(const uint8_t* const pixelData, const uint64_t timeStampNs)
	{
#if ENABLE_TRACE
		auto start = TRACE_TIMESTAMP;
#endif
		TRACE("H264Encoder::Encode begin");
		const DWORD bufferSize =
#if USE_TEST_CONTENT
			static_cast<DWORD>(m_TempImage.size())
#else
			m_Width * m_Height * 3 / 2 // NV12 size.
#endif
			;

		IMFMediaBufferPtr mediaBuffer;
		IMFSamplePtr mediaSample;
		if (!m_InputSample)
		{
			CHECK_HR_RET(MFCreateSample(&m_InputSample), "Could not create MFSample");
			CHECK_HR_RET(MFCreateMemoryBuffer(bufferSize, &mediaBuffer), "Could not create memory buffer");
			CHECK_HR_RET(m_InputSample->AddBuffer(mediaBuffer.GetInterfacePtr()), "Could not add buffer to sample");
		}
		else
			CHECK_HR_RET(m_InputSample->GetBufferByIndex(0, &mediaBuffer), "Could not get input buffer");

		mediaSample = m_InputSample;
		TRACE("IMFMediaBuffer::Lock");
		BYTE* dataPtr = nullptr;
		CHECK_HR_RET(mediaBuffer->Lock(&dataPtr, nullptr, nullptr), "Could not lock media buffer");
		TRACE("memcpy");
#if USE_TEST_CONTENT
		memcpy(dataPtr, m_TempImage.data(), m_TempImage.size());
#elif USE_MONOCHROME_CONTENT
		const int pixCount = m_Width * m_Height;
		memcpy(dataPtr, pixelData, pixCount);
		memset(dataPtr + pixCount, 127, pixCount / 2);
#else
		memcpy(dataPtr, pixelData, bufferSize);
#endif
		TRACE("IMFMediaBuffer::Unlock");
		mediaBuffer->Unlock();
		TRACE("IMFMediaBuffer::SetCurrentLength");
		CHECK_HR_RET(mediaBuffer->SetCurrentLength(bufferSize), "Could not set buffer length");

		TRACE("IMFSample::SetSampleTime");
		const LONGLONG sampleTimeHNS = timeStampNs / 100;
		CHECK_HR_RET(mediaSample->SetSampleTime(sampleTimeHNS), "Could not set sample time");

		TRACE("IMFSample::SetSampleDuration");
		const LONGLONG frameDurationHNS = m_FrameRateDenominator * 100000000 / m_FrameRateNumerator;
		CHECK_HR_RET(mediaSample->SetSampleDuration(frameDurationHNS), "Could not set sample duration");

		TRACE("IMFTransform::ProcessInput");
		HRESULT hr = m_Transform->ProcessInput(0, mediaSample, 0);
		if (!SUCCEEDED(hr))
		{
			TRACE("The resampler H264 ProcessInput call failed");
			return false;
		}

		TRACE("H264Encoder::Encode done: " << (TRACE_TIMESTAMP - start));
		return true;
    }

	bool BeginConsume(uint32_t& sizeOut)
	{
#if ENABLE_TRACE
		auto start = TRACE_TIMESTAMP;
#endif
		TRACE("H264Encoder::BeginConsume");
		// Make sure the previous consume is completed before starting another one.
		if (m_OutputData.pSample != nullptr)
		{
			TRACE("Previous output buffer not finished consuming.");
			return false;
		}

		{
			TRACE("GetOutputStatus");
			DWORD mftOutFlags = 0;
			CHECK_HR_RET(m_Transform->GetOutputStatus(&mftOutFlags), "H264 MFT GetOutputStatus failed");
			if (mftOutFlags != MFT_OUTPUT_STATUS_SAMPLE_READY)
				return false;
		}

		TRACE("GetOutputStreamInfo");
		MFT_OUTPUT_STREAM_INFO outputStreamInfo = {};
		CHECK_HR_RET(m_Transform->GetOutputStreamInfo(0, &outputStreamInfo), "Failed to get output stream info from H264 MFT");

		m_OutputData = {};
		if (!(outputStreamInfo.dwFlags & MFT_OUTPUT_STREAM_PROVIDES_SAMPLES))
		{
			if (!m_OutputBuffer)
			{
				TRACE("MFCreateAlignedMemoryBuffer");
				CHECK_HR_RET(MFCreateAlignedMemoryBuffer(outputStreamInfo.cbSize, outputStreamInfo.cbAlignment, &m_OutputBuffer), "Failed to create aligned memory buffer");
			}

			if (!m_OutputSample)
			{
				TRACE("MFCreateSample and AddBuffer");
				CHECK_HR_RET(MFCreateSample(&m_OutputSample), "Failed to create output sample");
				CHECK_HR_RET(m_OutputSample->AddBuffer(m_OutputBuffer), "Failed to add sample to buffer");
			}

			TRACE("GetMaxLength");
			DWORD maxLength = 0;
			CHECK_HR_RET(m_OutputBuffer->GetMaxLength(&maxLength), "Failed to bet media buffer max length");
			if (maxLength < outputStreamInfo.cbSize)
			{
				TRACE("RemoveAllBuffers");
				CHECK_HR_RET(m_OutputSample->RemoveAllBuffers(), "Failed to remove buffers from sample");
				TRACE("IMFMediaBuffer::Release");
				m_OutputBuffer.Detach()->Release();
				TRACE("MFCreatAlignedMemoryBuffer");
				CHECK_HR_RET(MFCreateAlignedMemoryBuffer(outputStreamInfo.cbSize, outputStreamInfo.cbAlignment, &m_OutputBuffer), "Failed to create larger aligned memory buffer");
				TRACE("AddBuffer");
				CHECK_HR_RET(m_OutputSample->AddBuffer(m_OutputBuffer), "Failed to add buffer to sample");
			}
			m_OutputData.pSample = m_OutputSample.GetInterfacePtr();
		}

		TRACE("GetNextEncodedBuffer");
		if (!GetNextEncodedBuffer())
			return false;

		// If the output buffer is not set at this point, it's because the transform provide IMFSamples, so it's our
		// job to extract the buffer from the sample.
		if (!m_OutputBuffer)
		{
			TRACE("ConvertToContiguousBuffer");
			CHECK_HR_RET(m_OutputData.pSample->ConvertToContiguousBuffer(&m_OutputBuffer), "Could not obtain IMFMediaBuffer from MFT sample");
		}

		TRACE("GetCurrentLength");
		DWORD length = 0;
		CHECK_HR_RET(m_OutputBuffer->GetCurrentLength(&length), "Could not obtain IMFMediaBuffer length");

		// Don't expose the Annex B prefix: this API returns raw NAL units.
		// FIXME: Trying something. If we expose the prefix as well, this triggers H264 slicing in the
		// client, which will do the same job we'd have to do on the RTP packetization side. Give it a try...
		sizeOut = length; //  -kAnnexBPrefixSize;
		TRACE("H264Encoder::BeginConsume done: " << (TRACE_TIMESTAMP - start));
		return true;
	}
    
    bool EndConsume(uint8_t* const dst, uint64_t& timeStampNsOut, bool& isKeyFrame)
    {
		// If there is no sample in the output data, it's because BeginConsume wasn't called yet.
		if (m_OutputData.pSample == nullptr)
			return false;

		// Transfer ownership to a local _com_ptr_t to release the buffer and sample no matter how we 
		// exit the scope.
		IMFSamplePtr outputSample(m_OutputData.pSample);
		IMFMediaBufferPtr outputBuffer;

		// When we own the sample, we also own the output buffer, so have the temp outputBuffer add a ref
		// so the release cancels out on scope exit.
		if (m_OutputSample)
			outputBuffer.Attach(m_OutputBuffer.GetInterfacePtr(), true);
		else
			outputBuffer.Attach(m_OutputBuffer.Detach());
		m_OutputData = {};

		UINT32 blobSize = 0;
		HRESULT hr = outputSample->GetBlobSize(MF_NALU_LENGTH_INFORMATION, &blobSize);
		if (hr == S_OK)
			TRACE("Nalu lenght information blob size: " << blobSize)
		else
			TRACE("Nalu lenght information not available.");

		DWORD bufLength = 0;
		CHECK_HR_RET(outputBuffer->GetCurrentLength(&bufLength), "Get buffer length failed.\n");
		uint8_t* src = nullptr;
		CHECK_HR_RET(outputBuffer->Lock(&src, NULL, NULL), "Could not lock buffer");
		TRACE("Lock got " << bufLength << " bytes.");
#if ENABLE_TRACE
		for (DWORD i = 0; i < min(60, bufLength); ++i)
			TRACE("Byte " << i << ": [" << TRACE_HEX(static_cast<unsigned int>(src[i])) << "]");
#endif
		// FIXME: Trying something. See equivalent FIXME in BeginConsume for actually exposing the prefix.
		const size_t offsetInBuffer = 0; //  kAnnexBPrefixSize;
		memcpy(dst, src + offsetInBuffer, bufLength - offsetInBuffer);
		CHECK_HR_RET(outputBuffer->Unlock(), "Could not unlock buffer");
		LONGLONG sampleTime = 0;
		CHECK_HR_RET(outputSample->GetSampleTime(&sampleTime), "Could not get sample time");
		timeStampNsOut = sampleTime;

		UINT32 isKey = 0;
		hr = outputSample->GetUINT32(MFSampleExtension_CleanPoint, &isKey);
		if (hr != S_OK)
		{
			TRACE("Could not get sample flags: " << TRACE_HEX(hr));
		}
		else
			isKeyFrame = isKey != 0;

		TRACE("H264Encoder::EndConsume isKeyFrame: " << isKey);
		if (!isKeyFrame)
			return true;

		// Got a keyframe, refresh the sps/pps as they may change as a result of format change (although 
		// as of this writing we aren't dynamically changing any config parameter).
		return ParseSpsPps();
	}

private:

	bool ParseSpsPps()
	{
        IMFMediaTypePtr mediaType;
	    CHECK_HR_RET(m_Transform->GetOutputCurrentType(0, &mediaType), "Could not get transform output media type");
		return ParseSpsPps(mediaType);
	}

	bool ParseSpsPps(IMFMediaTypePtr& mediaType)
	{
		UINT32 sequenceHeaderDataSize = 0;
		CHECK_HR_RET(mediaType->GetBlobSize(MF_MT_MPEG_SEQUENCE_HEADER, &sequenceHeaderDataSize),
			"Failed to get sequence header data size");
		if (sequenceHeaderDataSize == 0)
		{
			TRACE("Could not get sequence header size.");
			return false;
		}

		std::vector<uint8_t> sequenceHeaderData(sequenceHeaderDataSize);
		CHECK_HR_RET(mediaType->GetBlob(MF_MT_MPEG_SEQUENCE_HEADER, sequenceHeaderData.data(), sequenceHeaderDataSize, NULL),
			"Failed to get sequence header data");

		if (sequenceHeaderData.size() < 8)
		{
			TRACE("Sequence header size " << sequenceHeaderData.size() << " insufficient to contain NAL prefix and SPS+PPS.");
			return false;
		}

		// Media Foundation H.264 encoder uses the 4-bytes Annex B format (not AVCC), so each NALU is
		// prefixed with 0x00000001.
		const std::array<uint8_t, 4> naluHeader = { 0x00, 0x00, 0x00, 0x01 };
		auto firstNaluIt = std::search(std::begin(sequenceHeaderData), std::end(sequenceHeaderData),
			std::begin(naluHeader), std::end(naluHeader));

		if (firstNaluIt == sequenceHeaderData.end())
		{
			TRACE("Could not find first nalu in sequence header.");
			return false;
		}

		if (std::distance(firstNaluIt, sequenceHeaderData.end()) <= static_cast<ptrdiff_t>(naluHeader.size()))
		{
			TRACE("first nalu size too small.");
			return false;
		}

		// Search the second nalu from the point where the first header ends.
		auto secondNaluIt = firstNaluIt + naluHeader.size();
		secondNaluIt = std::search(secondNaluIt, std::end(sequenceHeaderData),
		std::begin(naluHeader), std::end(naluHeader));
		if (secondNaluIt == sequenceHeaderData.end())
		{
			TRACE("Could not find secondNalu in sequence header.");
			return false;
		}

		if (std::distance(secondNaluIt, sequenceHeaderData.end()) <= 4)
		{
			TRACE("second nalu size too small.");
			return false;
		}

		// Skip the nalu header to just keep the data.
		firstNaluIt += naluHeader.size();
		std::vector<uint8_t> firstNalu(firstNaluIt, secondNaluIt);

		// Skip the nalu header to just keep the data.
		secondNaluIt += naluHeader.size();
		std::vector<uint8_t> secondNalu(secondNaluIt, sequenceHeaderData.end());

		// Header is supposed to contain one SPS and one PPS nalu. Although they seem to be always
		// in SPS-PPS order, ther doesn't seem to be a guarantee for this so we're detecting the
		// type. Using format information from
		// https://yumichan.net/video-processing/video-compression/introduction-to-h264-nal-unit/
		const uint8_t NALU_TYPE_MASK = 0x1F;
		const uint8_t firstNaluType = firstNalu[0] & NALU_TYPE_MASK;
		const uint8_t secondNaluType = secondNalu[0] & NALU_TYPE_MASK;

		const uint8_t SPS_NALU_TYPE = 0x07;
		const uint8_t PPS_NALU_TYPE = 0x08;
		if (firstNaluType == SPS_NALU_TYPE)
			m_Sps.swap(firstNalu);
		else if (firstNaluType == PPS_NALU_TYPE)
			m_Pps.swap(firstNalu);
		else
		{
			TRACE("First nalu type " << (int)firstNaluType << " not sps (7) nor pps (8)");
			return false;
		}

		if (secondNaluType == SPS_NALU_TYPE)
			m_Sps.swap(secondNalu);
		else if (secondNaluType == PPS_NALU_TYPE)
			m_Pps.swap(secondNalu);
		else
		{
			TRACE("Second nalu type " << secondNaluType << " not sps (7) nor pps (8)");
			return false;
		}

		if (m_Sps.empty())
		{
			TRACE("Sps not found.");
			return false;
		}

		if (m_Pps.empty())
		{
			TRACE("Pps not found.");
			return false;
		}

		return true;
	}

	bool GetNextEncodedBuffer()
	{
		DWORD processOutputStatus = 0;
		HRESULT mftProcessOutput = m_Transform->ProcessOutput(0, 1, &m_OutputData, &processOutputStatus);
		if (mftProcessOutput == MF_E_TRANSFORM_NEED_MORE_INPUT)
			return false;

		CHECK_HR_RET(mftProcessOutput, "Error in MFT ProcessOutput");
		return true;
	}

	uint32_t               m_FrameRateNumerator = 0;
	uint32_t               m_FrameRateDenominator = 0;
	uint32_t               m_Width = 0;
	uint32_t               m_Height = 0;
	IMFTransformPtr        m_Transform;
	ICodecAPIPtr           m_Codec;
	bool                   m_IsTransformAsync = false;
	bool                   m_IsTransformHardware = false;
	IMFSamplePtr           m_InputSample;
	MFT_OUTPUT_DATA_BUFFER m_OutputData = {};
	IMFMediaBufferPtr      m_OutputBuffer;
	IMFSamplePtr           m_OutputSample;
	std::vector<uint8_t>   m_Sps;
	std::vector<uint8_t>   m_Pps;
#if USE_TEST_CONTENT
	std::vector<uint8_t>   m_TempImage;
#endif
};

#if ENABLE_TRACE
static std::once_flag InitLogOnce;
void InitLog()
{
	std::array<char, 32768> home;
	DWORD length = GetEnvironmentVariableA("USERPROFILE", home.data(), static_cast<DWORD>(home.size()));
	if (length == 0)
		return;
	std::string logPath(home.data());
	logPath.append("\\H264Encoder.log");
	g_logFile.open(logPath, std::fstream::out | std::fstream::trunc);
}
#endif

#define PINVOKE_ENTRY_POINT extern "C" __declspec(dllexport)

PINVOKE_ENTRY_POINT H264Encoder* Create(uint32_t width, uint32_t height, uint32_t frameRateNumerator, uint32_t frameRateDenominator, uint32_t averageBitRate, uint32_t gopSize)
{
#if ENABLE_TRACE
	std::call_once(InitLogOnce, InitLog);
#endif

	std::unique_ptr<H264Encoder> encoder(new H264Encoder());

	if (encoder->Initialize(width, height, frameRateNumerator, frameRateDenominator, averageBitRate, gopSize))
		return encoder.release();

	return nullptr;
}

PINVOKE_ENTRY_POINT bool Destroy(H264Encoder* encoder)
{
	delete encoder;
	return encoder != nullptr;
}

PINVOKE_ENTRY_POINT uint32_t GetSps(H264Encoder* encoder, uint8_t* spsOut)
{
	return encoder == nullptr ? 0 : encoder->GetSps(spsOut);
}

PINVOKE_ENTRY_POINT uint32_t GetPps(H264Encoder* encoder, uint8_t* ppsOut)
{
	return encoder == nullptr ? 0 : encoder->GetPps(ppsOut);
}

PINVOKE_ENTRY_POINT bool Encode(H264Encoder* encoder, uint8_t* pixelData, uint64_t timeStampNs)
{
	return encoder != nullptr && encoder->Encode(pixelData, timeStampNs);
}

PINVOKE_ENTRY_POINT bool BeginConsume(H264Encoder* encoder, uint32_t* sizeOut)
{
	return encoder != nullptr && sizeOut != nullptr && encoder->BeginConsume(*sizeOut);
}

PINVOKE_ENTRY_POINT bool EndConsume(H264Encoder* encoder, uint8_t* dst, uint64_t* timeStampNsOut, bool* isKeyFrameOut)
{
	return encoder != nullptr && dst != nullptr && timeStampNsOut != nullptr && isKeyFrameOut != nullptr && 
		encoder->EndConsume(dst, *timeStampNsOut, *isKeyFrameOut);
}
