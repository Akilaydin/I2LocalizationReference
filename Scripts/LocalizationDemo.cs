using UnityEngine;
using UnityEngine.UI;

namespace LocalizationExtension
{
	public class LocalizationDemo : MonoBehaviour
	{
		[SerializeField] private LocalizationReference _reference;

		private Text _text;

		private void Update()
		{
			Debug.Log(_reference);
		}
	}
}
