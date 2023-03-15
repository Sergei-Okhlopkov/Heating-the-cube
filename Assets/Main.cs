using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

public class Main : MonoBehaviour
{
    [SerializeField] GameObject electron;
    [SerializeField] Transform folder;
    [SerializeField] new GameObject camera;
    [SerializeField] public GameObject cam;
    [SerializeField] double L = 10.0, H = 0.1, TAU = 0.001, tmax = 10.0, T = 100, ALPHA = 0.6, r, timeStart, timeEnd;
    [SerializeField] int N;
    [SerializeField] double[,,] cube, cubeNew;
    [SerializeField] GameObject lastSphere;
    [SerializeField] bool end = false;

    void Start()
    {
        N = (int)(L / H) + 1;
        cam = Instantiate(camera, new Vector3(0, N / 2, -N * 2), new Quaternion(0, 0, 0, 0));
        cam.GetComponent<Camera>().orthographicSize = N;

        r = TAU * ALPHA * ALPHA / (H * H); //Постоянный коэффициент

        cube = new double[N, N, N];
        cubeNew = new double[N, N, N];

        //граница 1 - ГУ второго рода
        for (int i = 0; i < N; i++)
            for (int j = 0; j < N; j++)
            {
                cubeNew[i, 0, j] = cube[i, 1, j]; // граница 1
                cubeNew[i, j, N - 1] = cube[i, j, N - 2]; // граница 2
                cubeNew[i, N - 1, j] = cube[i, N - 2, j]; // граница 3
                cubeNew[N - 1, i, j] = cube[N - 2, i, j]; // граница 5
                cubeNew[0, i, j] = cube[1, i, j]; // граница 6
                cubeNew[i, j, 0] = T; // граница 4

            }

        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                for (int k = 0; k < N; k++)
                {
                    GameObject elect = Instantiate(electron, new Vector3(i, j, k), new Quaternion(0, 0, 0, 0));
                    elect.transform.SetParent(folder);
                    elect.GetComponent<MeshRenderer>().material.color = new Color((float)(cube[i, j, k] / 100), 0, 0);
                    elect.name = Convert.ToString($"el{i * N * N + j * N + k}");
                }
            }
        }

        timeStart = Time.time;
        folder.transform.Rotate(new Vector3(-110, 0, 45));
    }

    void Calculate()
    {
        N = (int)(L / H) + 1;

        for (double time = 0; time < tmax; time += TAU)
        {

            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                {
                    cubeNew[i, 0, j] = cube[i, 1, j]; // граница 1
                    cubeNew[i, j, N - 1] = cube[i, j, N - 2]; // граница 2
                    cubeNew[i, N - 1, j] = cube[i, N - 2, j]; // граница 3
                    cubeNew[N - 1, i, j] = cube[N - 2, i, j]; // граница 5
                    cubeNew[0, i, j] = cube[1, i, j]; // граница 6
                    cubeNew[i, j, 0] = T; // граница 4

                }

            //основные вычисления

            for (int i = 1; i < N - 1; i++)
                for (int j = 1; j < N - 1; j++)
                    for (int k = 1; k < N - 1; k++)
                    {
                        cubeNew[i, j, k] = cube[i, j, k] + r * (cube[i + 1, j, k] + cube[i - 1, j, k] + cube[i, j + 1, k] + cube[i, j - 1, k] +
                            cube[i, j, k + 1] + cube[i, j, k - 1] - 6 * cube[i, j, k]);
                    }


            //переприсваивание 
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    for (int k = 0; k < N; k++)
                        cube[i, j, k] = cubeNew[i, j, k];
        }

        for (int i = 0; i < N; i++)//округляю значения чтоб покрасивше было
        {
            for (int j = 0; j < N; j++)
            {
                for (int k = 0; k < N; k++)
                {
                    cube[i, j, k] = Math.Round(cube[i, j, k], 5);
                }
            }
        }

        //беру все объекты с тэгом sphereColor
        GameObject[] objects = GameObject.FindGameObjectsWithTag("sphereColor");

        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                for (int k = 0; k < N; k++)
                {
                    double min, max;

                    if (cube[i, j, k] <= 35.0)
                    {
                        min = 0.0;
                        max = 35.0;
                        float normalized = (float)((cube[i, j, k] - min) / (max - min));
                        objects[i * N * N + j * N + k].GetComponent<MeshRenderer>().material.color = new Color(0, 0, 1 - normalized);
                    }

                    if (cube[i, j, k] > 35.0 && cube[i, j, k] <= 70)
                    {
                        min = 35.0;
                        max = 70.0;
                        float normalized = (float)((cube[i, j, k] - min) / (max - min));
                        objects[i * N * N + j * N + k].GetComponent<MeshRenderer>().material.color = new Color(normalized, normalized, 0);
                    }

                    if (cube[i, j, k] > 70.0)
                    {
                        min = 70.0;
                        max = 100.0;
                        float normalized = (float)((cube[i, j, k] - min) / (max - min));
                        objects[i * N * N + j * N + k].GetComponent<MeshRenderer>().material.color = new Color(normalized, 1 - normalized, 0);
                    }

                   // objects[i * N * N + j * N + k].GetComponent<MeshRenderer>().material.color = new Color((float)(cube[i, j, k] / 100), 0, 0);
                }
            }
        }

        lastSphere = folder.transform.GetChild(cube.Length - 1).gameObject;


    }


    void Update()
    {
        Calculate();
    }
}
