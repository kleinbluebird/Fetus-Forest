using System.IO;
using UnityEngine;

namespace FetusForest.Control
{
	/// <summary>
	/// 最简存档管理器：仅保存/读取最远章节索引（0/1/2）
	/// </summary>
	public class MiniSaveManager : MonoBehaviour
	{
		public static MiniSaveManager Instance { get; private set; }
		
		[System.Serializable]
		private class SaveData { public int furthestChapter = 0; }
		
		[SerializeField] private string saveFileName = "ff_save.json";
		private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);
		
		private void Awake()
		{
			if (Instance != null && Instance != this) { Destroy(gameObject); return; }
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		
		public bool HasSave()
		{
			return File.Exists(SavePath);
		}
		
		public int LoadFurthestChapter()
		{
			if (!HasSave()) return -1;
			try
			{
				var json = File.ReadAllText(SavePath);
				var data = JsonUtility.FromJson<SaveData>(json);
				return Mathf.Clamp(data.furthestChapter, 0, 2);
			}
			catch { return -1; }
		}
		
		public void SaveNewGame()
		{
			SaveFurthestChapter(0);
		}
		
		public void SaveFurthestChapter(int chapterIndex)
		{
			var data = new SaveData { furthestChapter = Mathf.Clamp(chapterIndex, 0, 2) };
			var json = JsonUtility.ToJson(data, true);
			File.WriteAllText(SavePath, json);
		}
	}
}
