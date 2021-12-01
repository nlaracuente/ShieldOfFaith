using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshSurface))]
public class NavmeshController : MonoBehaviour
{
    NavMeshSurface surface;
    public NavMeshSurface Surface
    {
        get
        {
            if (surface == null)
                surface = GetComponent<NavMeshSurface>();

            return surface;
        }
    }

    private void Start()
    {
        Build();
    }

    public void Build()
    {
        Surface.BuildNavMesh();
    }
}
