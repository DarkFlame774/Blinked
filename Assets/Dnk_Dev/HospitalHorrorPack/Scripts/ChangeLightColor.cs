using UnityEngine;

public class ChangePointLightColors : MonoBehaviour
{
    public Color newColor = Color.white; // Set the desired color in the Inspector

    void Update()
    {
        // Find all Light components in the scene
        Light[] lights = FindObjectsOfType<Light>();

        foreach (Light light in lights)
        {
            // Check if the light is a Point Light
            if (light.type == LightType.Point)
            {
                light.color = newColor; // Change the color
            }
        }
    }
}
