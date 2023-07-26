using System.Collections;
using UnityEngine;

namespace Unity.CompanionAppCommon
{
    interface ICoroutineRunner
    {
        Coroutine StartCoroutine(IEnumerator coroutine);
        void StopCoroutine(Coroutine coroutine);
    }

    class CoroutineRunner : MonoBehaviour, ICoroutineRunner
    {
    }
}
