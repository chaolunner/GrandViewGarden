using System.Collections;
using UnityEditor;
using UnityEngine;

public class CarAnimation : MonoBehaviour
{
    private Animator car;

    public float rotation = -1;
    public float direction = -1;

    void Start()
    {
        car = gameObject.GetComponent<Animator>();

    }
    void Update()
    {
        if (car)
        {
            car.SetFloat("Direction", direction);
            car.SetFloat("Rotation", rotation);
        }
        if (direction == -1)
        {
            rotation += 0.01f;
            if (rotation >= 1)
            {
                direction = 1;
            }
        }
        if (direction == 1)
        {
            rotation -= 0.01f;
            if (rotation <= -1)
            {
                direction = -1;
            }
        }
    }
}
