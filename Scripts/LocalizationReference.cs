using I2.Loc;

using UnityEngine;

namespace LocalizationExtension
{

    #if UNITY_EDITOR
	using UnityEditor;

	using UnityEngine.UIElements;

	public class LocalizationReferenceField : VisualElement
	{
		public Label _labelField;
		public TextField _textField;
		public Label _localizedField;
		public Button _button;

		private StringSearchWindow _stringSearchWindow;

		public string value {
			get {
				return _textField.text;
			}

			set {
				_textField.value = value;
			}
		}

		public LocalizationReferenceField(string name, string label, string[] classNames,
			EventCallback<ChangeEvent<string>> onChanged, EventCallback<FocusOutEvent> onFocusOut, int maxLength = -1)
		{
			// load some data.. 
			var i2LocalizationAsset =
				AssetDatabase.LoadAssetAtPath<I2.Loc.LanguageSourceAsset>(
					"Assets/ResourcesStatic/Data/Resources/I2Languages.asset");
			var terms = i2LocalizationAsset.SourceData.GetTermsList();

			// create our fields 
			_labelField = new Label();
			_button = new Button();
			_textField = new TextField();
			_localizedField = new Label();

			this.name = name;

			AddToClassList("localization-container");

			if (classNames != null)
			{
				foreach (var className in classNames)
				{
					AddToClassList(className);
					_button.AddToClassList(className);
					_textField.AddToClassList(className);
					_labelField.AddToClassList(className);
				}
			}

			// customize text field 
			if (!string.IsNullOrEmpty(label)) _labelField.text = label;
			if (onChanged != null) _textField.RegisterValueChangedCallback(onChanged);
			if (onFocusOut != null) _textField.RegisterCallback(onFocusOut);

			_textField.RegisterValueChangedCallback(e => SetValueWithoutNotify(e.newValue));
			_textField.multiline = false;
			_textField.isPasswordField = false;
			_textField.maxLength = maxLength;

			_button.text = "[search]";
			_button.clickable.clicked += () =>
			{
				var terms = i2LocalizationAsset.SourceData.GetTermsList();

				_stringSearchWindow = EditorWindow.GetWindow<StringSearchWindow>();
				_stringSearchWindow.ShowCustom(terms, _textField.contentRect, SetValueWithoutNotify);
				_stringSearchWindow.Focus();
			};

			SetValueWithoutNotify(value);

			Add(_labelField);
			Add(_button);
			Add(_textField);
			Add(_localizedField);
		}

		public void SetValueWithoutNotify(string value)
		{
			_textField.SetValueWithoutNotify(value);

			// show localized too 
			_localizedField.text = "[null]";

			if (!string.IsNullOrEmpty(value))
			{
				var i2LocalizationAsset =
					AssetDatabase.LoadAssetAtPath<I2.Loc.LanguageSourceAsset>(
						"Assets/ResourcesStatic/Data/Resources/I2Languages.asset");
				var englishValue = i2LocalizationAsset.SourceData.GetTranslation(value, "english");
				if (!string.IsNullOrEmpty(englishValue))
				{
					_localizedField.text = englishValue;
				}
			}
		}

		public bool RegisterValueChangedCallback(EventCallback<ChangeEvent<string>> callback)
		{
			return _textField.RegisterValueChangedCallback(callback);
		}

		public bool UnregisterValueChangedCallback(EventCallback<ChangeEvent<string>> callback)
		{
			return _textField.UnregisterValueChangedCallback(callback);
		}
	}

	[CustomPropertyDrawer(typeof(LocalizationReference), true)]
	public class LocalizationReferencePropertyDrawer : PropertyDrawer
	{
		[System.NonSerialized] private StringSearchWindow _stringSearchWindow;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var keyProperty = property.FindPropertyRelative("key");

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			var shortcutRect = position;
			shortcutRect.width = 24f;

			var editButtonRect = shortcutRect;
			editButtonRect.x += shortcutRect.width;
			editButtonRect.width = 24f;

			var labelRect = position;
			labelRect.width -= editButtonRect.width + shortcutRect.width;
			labelRect.width /= 2;
			labelRect.x += editButtonRect.width + shortcutRect.width;

			var localizedNameRect = labelRect;
			localizedNameRect.x += labelRect.width;

			const string configsPath = "Assets/";

			var assetNames = AssetDatabase.FindAssets("I2Languages", new[] { configsPath });

			if (assetNames.Length == 0)
			{
				Debug.LogError($"No {nameof(I2.Loc.LanguageSourceAsset)} was found at path {configsPath}");
			}

			var settingsPath = AssetDatabase.GUIDToAssetPath(assetNames[0]);

			var i2LocalizationAsset = AssetDatabase.LoadAssetAtPath<I2.Loc.LanguageSourceAsset>(settingsPath);

			if (GUI.Button(editButtonRect, EditorGUIUtility.IconContent("Search On Icon")))
			{
				var terms = i2LocalizationAsset.SourceData.GetTermsList();

				_stringSearchWindow = EditorWindow.GetWindow<StringSearchWindow>();

				System.Action<string> callback = str =>
				{
					keyProperty.serializedObject.Update();
					keyProperty.stringValue = str;
					keyProperty.serializedObject.ApplyModifiedProperties();
				};

				_stringSearchWindow.ShowCustom(terms, position, callback);
				_stringSearchWindow.Focus();
			}

			// display a shortcut so we can jump there later 
			if (GUI.Button(shortcutRect, EditorGUIUtility.IconContent("d_ScriptableObject Icon")))
			{
				Selection.SetActiveObjectWithContext(i2LocalizationAsset, i2LocalizationAsset);
			}

			// draw the text field for the term 
			EditorGUI.BeginChangeCheck();
			{
				EditorGUI.showMixedValue = keyProperty.hasMultipleDifferentValues;
				var newStringValue = EditorGUI.TextField(labelRect, keyProperty.stringValue);
				EditorGUI.showMixedValue = false;

				if (newStringValue != keyProperty.stringValue)
				{
					keyProperty.stringValue = newStringValue;
				}
			}
			EditorGUI.EndChangeCheck();

			// draw the translated term 
			EditorGUI.BeginDisabledGroup(true);
			{
				var englishValue = i2LocalizationAsset.SourceData.GetTranslation(keyProperty.stringValue);
				if (string.IsNullOrEmpty(englishValue))
				{
					var style = new GUIStyle();
					style.richText = true;

					EditorGUI.TextField(localizedNameRect, "<color=red><b>null</b></color>", style);
				}
				else
				{
					EditorGUI.TextField(localizedNameRect, englishValue);
				}
			}
			EditorGUI.EndDisabledGroup();
		}
	}
    #endif
    
    [System.Serializable]
    public struct LocalizationReference
    {
	    public string key;
        
	    public LocalizationReference(string key)
	    {
		    this.key = key;
	    }

	    public override string ToString()
	    {
		    return (LocalizedString) key;
	    }

	    public static implicit operator string(LocalizationReference reference)
	    {
		    return reference.ToString();
	    }

	    public static implicit operator LocalizationReference(string key)
	    {
		    return new LocalizationReference(key);
	    }
    }
}
