using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptDirector : MonoBehaviour
{
    private ShmupInputActions inputActions;
    [SerializeField]
    private GameObject playerCharacter;  // TODO: 管理クラスに置き換える。

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var velocity = inputActions.Player.Move.ReadValue<Vector2>();
        playerCharacter.GetComponent<PlayerCharacterController>().Velocity = velocity;
    }

    void Awake()
    {
        inputActions = new ShmupInputActions();
        inputActions.Enable();
    }
}
