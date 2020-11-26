using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerPrefs))]
public class EntradaJugador : MonoBehaviour
{
    Jugador jugador;

    void Start()
    {
        jugador = GetComponent<Jugador>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 entradaDireccional= new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        jugador.EstablecerEntradaDireccional(entradaDireccional);
        if (Input.GetKeyDown(KeyCode.Space))
            jugador.AlSaltarEntradaAbajo();
        if (Input.GetKeyUp(KeyCode.Space))
            jugador.AlSaltarEntradaArriba();
    }
}
