using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] private GameObject[] maps;

    private void OnEnable()
    {
        UpdateMap();
    }

    public void UpdateMap()
    {
        var savedScenes = SaveData.Instance.sceneNames;

        for (int i = 0; i < maps.Length; i++)
        {
            string mapName = $"Map {i + 1}";
            bool isActive = savedScenes.Contains(mapName);
            maps[i].SetActive(isActive);
            Debug.Log($"Map {mapName} is {(isActive ? "active" : "inactive")}");
        }
    }
}