using UnityEngine;
using NavMeshPlus.Components;
using System.Collections;

// Скрипт для создания границ карты, совместимых с NavMeshSurface
// Создаёт невидимые стены, которые блокируют игрока и врагов
public class MapBoundary : MonoBehaviour
{
    [Header("Настройки границ")]
    [Tooltip("Размер карты (ширина x высота)")]
    [SerializeField] private Vector2 mapSize = new Vector2(50f, 50f);

    [Tooltip("Толщина стен границы")]
    [SerializeField] private float wallThickness = 2f;

    [Tooltip("Высота стен (для 2D можно оставить 1)")]
    [SerializeField] private float wallHeight = 1f;

    [Header("Слои")]
    [Tooltip("Слой для границ (должен блокировать NavMesh)")]
    [SerializeField] private string boundaryLayer = "Obstacle";

    [Header("NavMesh")]
    [Tooltip("Перестроить NavMesh после создания границ")]
    [SerializeField] private bool rebuildNavMesh = true;
    [Tooltip("Ссылка на NavMeshSurface (найдётся автоматически если не указана)")]
    [SerializeField] private NavMeshSurface navMeshSurface;

    [Header("Отладка")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);

    private void Start()
    {
        CreateBoundaries();

        // Перестраиваем NavMesh после создания границ
        if (rebuildNavMesh)
        {
            StartCoroutine(RebuildNavMeshDelayed());
        }
    }

    // Перестраиваем NavMesh с небольшой задержкой, чтобы все объекты успели инициализироваться
    private IEnumerator RebuildNavMeshDelayed()
    {
        yield return new WaitForEndOfFrame();

        if (navMeshSurface == null)
        {
            navMeshSurface = FindFirstObjectByType<NavMeshSurface>();
        }

        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("MapBoundary: NavMesh перестроен с учётом границ");
        }
        else
        {
            Debug.LogWarning("MapBoundary: NavMeshSurface не найден! Границы могут не работать для NavMesh агентов.");
        }
    }

    // Создаёт 4 стены вокруг карты
    private void CreateBoundaries()
    {
        // Верхняя стена
        CreateWall("Wall_Top",
            new Vector3(0, mapSize.y / 2 + wallThickness / 2, 0),
            new Vector3(mapSize.x + wallThickness * 2, wallThickness, wallHeight));

        // Нижняя стена
        CreateWall("Wall_Bottom",
            new Vector3(0, -mapSize.y / 2 - wallThickness / 2, 0),
            new Vector3(mapSize.x + wallThickness * 2, wallThickness, wallHeight));

        // Левая стена
        CreateWall("Wall_Left",
            new Vector3(-mapSize.x / 2 - wallThickness / 2, 0, 0),
            new Vector3(wallThickness, mapSize.y + wallThickness * 2, wallHeight));

        // Правая стена
        CreateWall("Wall_Right",
            new Vector3(mapSize.x / 2 + wallThickness / 2, 0, 0),
            new Vector3(wallThickness, mapSize.y + wallThickness * 2, wallHeight));

        Debug.Log($"MapBoundary: Созданы границы карты размером {mapSize.x}x{mapSize.y}");
    }

    // Создаёт одну стену
    private void CreateWall(string wallName, Vector3 localPosition, Vector3 size)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.SetParent(transform);
        wall.transform.localPosition = localPosition;

        // Добавляем BoxCollider2D для физики
        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(size.x, size.y);

        // Устанавливаем слой
        int layerIndex = LayerMask.NameToLayer(boundaryLayer);
        if (layerIndex >= 0)
        {
            wall.layer = layerIndex;
        }
        else
        {
            Debug.LogWarning($"MapBoundary: Слой '{boundaryLayer}' не найден! Используется Default");
        }

        // Добавляем NavMeshModifier для блокировки NavMesh
        // NavMeshModifier будет помечать эту область как непроходимую
        var navMeshModifier = wall.AddComponent<NavMeshModifier>();
        navMeshModifier.overrideArea = true;
        navMeshModifier.area = 1; // Not Walkable area (обычно индекс 1)
    }

    // Получить размер карты
    public Vector2 GetMapSize()
    {
        return mapSize;
    }

    // Проверить, находится ли позиция внутри границ
    public bool IsInsideBounds(Vector3 position)
    {
        Vector3 center = transform.position;
        float halfWidth = mapSize.x / 2;
        float halfHeight = mapSize.y / 2;

        return position.x >= center.x - halfWidth &&
               position.x <= center.x + halfWidth &&
               position.y >= center.y - halfHeight &&
               position.y <= center.y + halfHeight;
    }

    // Получить ближайшую точку внутри границ
    public Vector3 ClampToBounds(Vector3 position)
    {
        Vector3 center = transform.position;
        float halfWidth = mapSize.x / 2;
        float halfHeight = mapSize.y / 2;

        return new Vector3(
            Mathf.Clamp(position.x, center.x - halfWidth, center.x + halfWidth),
            Mathf.Clamp(position.y, center.y - halfHeight, center.y + halfHeight),
            position.z
        );
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;

        // Рисуем границы карты
        Vector3 center = transform.position;
        Vector3 size = new Vector3(mapSize.x, mapSize.y, 0.1f);

        // Заполненный прямоугольник (границы)
        Gizmos.DrawWireCube(center, size);

        // Рисуем стены
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);

        // Верхняя
        Gizmos.DrawCube(
            center + new Vector3(0, mapSize.y / 2 + wallThickness / 2, 0),
            new Vector3(mapSize.x + wallThickness * 2, wallThickness, 0.1f));

        // Нижняя
        Gizmos.DrawCube(
            center + new Vector3(0, -mapSize.y / 2 - wallThickness / 2, 0),
            new Vector3(mapSize.x + wallThickness * 2, wallThickness, 0.1f));

        // Левая
        Gizmos.DrawCube(
            center + new Vector3(-mapSize.x / 2 - wallThickness / 2, 0, 0),
            new Vector3(wallThickness, mapSize.y + wallThickness * 2, 0.1f));

        // Правая
        Gizmos.DrawCube(
            center + new Vector3(mapSize.x / 2 + wallThickness / 2, 0, 0),
            new Vector3(wallThickness, mapSize.y + wallThickness * 2, 0.1f));
    }
#endif
}