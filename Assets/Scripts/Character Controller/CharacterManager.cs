using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    static CharacterManager instance;
    public static CharacterManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<CharacterManager>();
            return instance;
        }
    }

    public Animator animator;
    public GameObject model;
}