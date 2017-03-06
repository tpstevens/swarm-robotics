using UnityEngine;

namespace Utilities
{
    public class WorldspaceUIFactory
    {
        public static GameObject createQuad(string name, Rect rect, Material mat)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);

            quad.transform.name = name;
            quad.transform.localScale = new Vector3(rect.width, rect.height, 1);
            quad.transform.position = new Vector3(rect.center.x, 0.01f, rect.center.y);
            quad.transform.rotation = Quaternion.AngleAxis(90, Vector3.right);

            quad.GetComponent<Renderer>().material = mat;

            return quad;
        }
    }
}
