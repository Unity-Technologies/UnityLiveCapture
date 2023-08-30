#pragma once

#include <thread>
#include <iostream>

namespace NvencPlugin
{
    class NvThread
    {
    public:
        NvThread() = default;
        NvThread(const NvThread&) = delete;
        NvThread& operator=(const NvThread& other) = delete;

        inline NvThread(std::thread&& thread) : m_Thread(std::move(thread))
        {
        }

        inline NvThread(NvThread&& thread) noexcept : m_Thread(std::move(thread.m_Thread))
        {
        }

        inline NvThread& operator=(NvThread&& other) noexcept
        {
            m_Thread = std::move(other.m_Thread);
            return *this;
        }

        inline ~NvThread()
        {
            join();
        }

        inline void join()
        {
            if (m_Thread.joinable())
            {
                m_Thread.join();
            }
        }

    private:
        std::thread m_Thread;
    };

    struct NvSpinlock
    {
    public:
        void lock()
        {
            for (;;)
            {
                // Optimistically assume the lock is free on the first try.
                if (!m_Lock.exchange(true, std::memory_order_acquire))
                    break;

                // Wait for lock to be released without generating cache misses.
                while (m_Lock.load(std::memory_order_relaxed));
            }
        }

        void unlock() noexcept
        {
            m_Lock.store(false, std::memory_order_release);
        }

    private:
        std::atomic<bool> m_Lock = { 0 };
    };
}
