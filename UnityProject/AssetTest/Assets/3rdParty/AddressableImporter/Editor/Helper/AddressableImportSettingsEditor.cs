/// <summary>
/// ButtonMethodAttribute,
/// modified from https://github.com/Deadcows/MyBox/blob/master/Attributes/ButtonMethodAttribute.cs
/// </summary>
using UnityEngine;

namespace UnityAddressableImporter.Helper.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Editor.Helper;
    using UnityEditor;


    [CustomEditor(typeof(AddressableImportSettings), true), CanEditMultipleObjects]
    public class AddressableImportSettingsEditor : Editor
    {
        private List<MethodInfo> _methods;
        private AddressableImportSettings _target;
#if ODIN_INSPECTOR
        private AddressableImporterOdinHandler _drawer;
#endif


        private void OnEnable()
        {
            _target = target as AddressableImportSettings;
#if ODIN_INSPECTOR
            _drawer = _drawer ?? new AddressableImporterOdinHandler();
#endif
            if (_target == null) return;
#if ODIN_INSPECTOR
            _drawer.Initialize(_target);
#endif
            _methods = AddressableImporterMethodHandler.CollectValidMembers(_target.GetType());

        }

        private void OnDisable()
        {
#if ODIN_INSPECTOR
            _drawer.Dispose();
#endif
        }

        public override void OnInspectorGUI()
        {
            DrawBaseEditor();

#if !ODIN_INSPECTOR
            if (_methods == null) return;

            AddressableImporterMethodHandler.OnInspectorGUI(_target, _methods);
#endif

            serializedObject.ApplyModifiedProperties();

        }

        private void DrawBaseEditor()
        {
#if ODIN_INSPECTOR
			_drawer.Draw();
#else
            base.OnInspectorGUI();
#endif
        }
    }
}