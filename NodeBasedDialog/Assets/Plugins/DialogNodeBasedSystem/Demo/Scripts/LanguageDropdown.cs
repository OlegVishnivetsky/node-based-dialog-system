using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if UNITY_LOCALIZATION
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
#endif

namespace DialogNodeBasedSystem.Demo.Scripts
{
    public class LanguageDropdown : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _languageDropdown;
        
#if UNITY_LOCALIZATION
        private readonly List<Locale> _availableLocales = new();
#endif
        
        private void Start() => Initialize();

        private void Initialize()
        {
#if UNITY_LOCALIZATION
            if (!LocalizationSettings.InitializationOperation.IsDone)
                LocalizationSettings.InitializationOperation.Completed += _ => SetupLanguageDropdown();
            else
                SetupLanguageDropdown();
#else
            // Handle case when localization is not available
            _languageDropdown.ClearOptions();
            _languageDropdown.AddOptions(new List<string> { "Localization not available" });
            _languageDropdown.interactable = false;
            Debug.LogWarning("Localization package is not available. Language dropdown will not function.");
#endif
        }

#if UNITY_LOCALIZATION
        private void SetupLanguageDropdown()
        {
            _availableLocales.Clear();
            _languageDropdown.ClearOptions();
            
            List<string> options = new List<string>();
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                options.Add(locale.LocaleName);
                _availableLocales.Add(locale);
            }
            
            _languageDropdown.AddOptions(options);
            
            if (LocalizationSettings.SelectedLocale != null)
            {
                int currentIndex = _availableLocales.IndexOf(LocalizationSettings.SelectedLocale);
                if (currentIndex >= 0)
                    _languageDropdown.value = currentIndex;
            }
            
            _languageDropdown.onValueChanged.AddListener(OnLanguageSelected);
        }
        
        private void OnLanguageSelected(int index)
        {
            if (index >= 0 && index < _availableLocales.Count)
                LocalizationSettings.SelectedLocale = _availableLocales[index];
        }
#endif
    }
}