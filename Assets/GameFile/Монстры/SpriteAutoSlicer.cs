using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SliceLargestSprite : EditorWindow
{
    [MenuItem("Assets/Slice Largest Sprite", false, 200)]
    public static void SliceSelectedTexture()
    {
        // Проверяем, что выбран ассет типа Texture2D
        Texture2D texture = Selection.activeObject as Texture2D;
        if (texture == null)
        {
            Debug.LogError("Выберите PNG-текстуру (Texture2D) в проекте.");
            return;
        }

        // Получаем путь к ассету и его TextureImporter
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("Не удалось получить TextureImporter.");
            return;
        }

        // Включаем режим чтения, если он не включён
        if (!importer.isReadable)
        {
            importer.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        // Загружаем пиксели текстуры
        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;
        bool[] visited = new bool[pixels.Length];
        List<RectInt> regions = new List<RectInt>();

        // Задаем порог прозрачности (уменьшили порог, чтобы захватывались слабые пиксели)
        float alphaThreshold = 0.05f;

        // Обходим все пиксели для поиска связных областей (8-связность)
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (!visited[index] && pixels[index].a > alphaThreshold)
                {
                    RectInt region = FloodFill8Connected(x, y, width, height, pixels, visited, alphaThreshold);
                    // Игнорируем очень маленькие области (шум)
                    if (region.width * region.height >= 10)
                        regions.Add(region);
                }
            }
        }

        if (regions.Count == 0)
        {
            Debug.LogWarning("Не найдено ни одной области с непрозрачными пикселями.");
            return;
        }

        // Выбираем область с наибольшей площадью
        RectInt largest = regions[0];
        foreach (var r in regions)
        {
            if (r.width * r.height > largest.width * largest.height)
                largest = r;
        }

        // Создаем метаданные спрайта для выбранного региона
        SpriteMetaData meta = new SpriteMetaData();
        meta.name = texture.name + "_Largest";
        // В SpriteMetaData координаты rect начинаются с нижнего левого угла
        meta.rect = new Rect(largest.x, largest.y, largest.width, largest.height);
        meta.alignment = (int)SpriteAlignment.Custom;
        meta.pivot = new Vector2(0.5f, 0f);

        // Устанавливаем режим импорта спрайтов на Multiple и назначаем единственный спрайт
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritesheet = new SpriteMetaData[] { meta };

        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        Debug.Log("Вырезан спрайт. Выбранный регион: " + meta.rect);
    }

    // Алгоритм 8-связного flood fill для определения связной области
    private static RectInt FloodFill8Connected(int startX, int startY, int width, int height, Color[] pixels, bool[] visited, float alphaThreshold)
    {
        int minX = startX, maxX = startX;
        int minY = startY, maxY = startY;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));

        // Массив смещений для 8-связности
        int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        while (queue.Count > 0)
        {
            Vector2Int p = queue.Dequeue();
            int x = p.x;
            int y = p.y;
            int index = y * width + x;
            if (visited[index])
                continue;
            visited[index] = true;

            // Обновляем границы области
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;

            for (int i = 0; i < 8; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    int nIndex = ny * width + nx;
                    if (!visited[nIndex] && pixels[nIndex].a > alphaThreshold)
                    {
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }

        return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
}
