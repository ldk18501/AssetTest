using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Qarth;

namespace Qarth.Addressable
{
    public class AddressResLoader
    {
        private string m_LoaderName;

        public AddressResLoader(string name)
        {
            m_LoaderName = name;

        }
    }
}