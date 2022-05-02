namespace LocalizationExtension
{

#if UNITY_EDITOR
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;

    public class StringSearchWindow
        : EditorWindow
    {
        [System.NonSerialized] private System.Action<string> _onSelectCallback;
        [System.NonSerialized] private EditorWindow _parent;
        [System.NonSerialized] private List<string> _values;
        [System.NonSerialized] private List<string> _results;
        [System.NonSerialized] private string _searchQuery;
        [System.NonSerialized] private Vector2 _scroll;
        [System.NonSerialized] private bool _lostFocus;

        public void ShowCustom(List<string> values, Rect rect, System.Action<string> onSelect)
        {
            _searchQuery = string.Empty;
            _values = new List<string>(values);
            _results = new List<string>(values);
            _onSelectCallback = onSelect;

            _parent = focusedWindow;

            var screenRect = rect;
            var screenSize = new Vector2(512, 256);

            screenRect.position = GUIUtility.GUIToScreenPoint(screenRect.position);

            ShowAsDropDown(screenRect, screenSize);
            Focus();

            GUI.FocusControl("filter");
        }

        private void OnGUI()
        {
            GUI.SetNextControlName("filter");

            var filterUpdate = GUILayout.TextField(_searchQuery);
            if (!filterUpdate.Equals(_searchQuery, System.StringComparison.Ordinal))
            {
                UpdateSearchResults(filterUpdate);
            }

            GUI.FocusControl("filter");

            _scroll = GUILayout.BeginScrollView(_scroll, GUIStyle.none, GUI.skin.verticalScrollbar);
            {
                for (int i = 0; i < _results.Count; ++i)
                {
                    var value = _results[i];
                    if (GUILayout.Button(value))
                    {
                        GUILayout.EndScrollView();

                        _onSelectCallback(value);
                        Close();

                        _parent.Repaint();
                        _parent.Focus();

                        return;
                    }
                }
            }

            GUILayout.EndScrollView();

            if (_lostFocus)
            {
                Close();
            }
        }

        public void OnLostFocus()
        {
            _lostFocus = true;
            this.Repaint();
        }

        private void UpdateSearchResults(string searchQuery)
        {
            _searchQuery = searchQuery;

            var filterLower = _searchQuery.ToLower();
            var filterSplit = new List<string>(filterLower.Split(' '));

            var candidates = new List<string>();

            for (int i = 0; i < _values.Count; ++i)
            {
                candidates.Add(_values[i]);
            }

            for (int i = 0; i < filterSplit.Count; ++i)
            {
                var filterToken = filterSplit[i];

                if (string.IsNullOrWhiteSpace(filterToken))
                {
                    continue;
                }

                for (int j = 0; j < candidates.Count; ++j)
                {
                    var candidate = candidates[j];
                    var candidateLower = candidate.ToLower();
                    if (candidateLower.Contains(filterToken) == false)
                    {
                        candidates.RemoveAt(j--);
                    }
                }
            }

            _results.Clear();

            for (int i = 0; i < candidates.Count; ++i)
            {
                _results.Add(candidates[i]);
            }
        }
    }

#endif
}