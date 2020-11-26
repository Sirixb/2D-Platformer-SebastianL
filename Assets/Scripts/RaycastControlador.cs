using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Script creado en Ep6

//movido de controlador2d en Ep6
[RequireComponent(typeof(BoxCollider2D))]
public class RaycastControlador : MonoBehaviour {

	public LayerMask mascaraColision;

	public const float anchoPiel=.015f;//const 015f//se vuelve publica en Ep6
    const float distEntreRayo = .25f;//Ep12
    [HideInInspector]
    public int conteoRayosHorizontales;//4 pero ya no es necesario ep12
    [HideInInspector]
    public int conteoRayosVerticales;

	[HideInInspector]//no es necesario se vean porque son calculados por una funcio abajo ep6
	public float espacioRayoHorizontal;//se vuelve publica en Ep6
	[HideInInspector]//dice que no es necesario se vean ep6
	public float espacioRayoVertical;//se vuelve publica en Ep6

	[HideInInspector]//dice que no es necesario se vean ep6
	public BoxCollider2D colisionador;
	public OrigenRayos origenRayos;
    //Ep11 se crea para que se llame antes el colisionador que necesita CamaraSeguimiento.cs
    public virtual void Awake()
    {
        colisionador = GetComponent<BoxCollider2D>();   
    }
    //todos los metodos se vuelven publicos en Ep6
    // se usa por primera vez el nombre virutal
    public virtual void Start(){
		CalcularEspacioRayos ();
	}
	//Episodio1
	public void ActualizarOrigenRayos(){
		Bounds limites = colisionador.bounds;
		limites.Expand (anchoPiel * -2);

		origenRayos.inferiorIzquierda = new Vector2 (limites.min.x, limites.min.y);
		origenRayos.inferiorDerecha = new Vector2 (limites.max.x, limites.min.y);
		origenRayos.superiorIzquierda = new Vector2 (limites.min.x, limites.max.y);
		origenRayos.superiorDerecha = new Vector2 (limites.max.x, limites.max.y);
	}

	//Episodio1
	public void CalcularEspacioRayos(){
		Bounds limites = colisionador.bounds;
		limites.Expand (anchoPiel * -2);

        //Ep12
        float anchoLimites = limites.size.x;
        float alturaLimites = limites.size.y;
        conteoRayosHorizontales = Mathf.RoundToInt(alturaLimites / distEntreRayo);
        conteoRayosVerticales = Mathf.RoundToInt(anchoLimites / distEntreRayo);
        //conteoRayosHorizontales = Mathf.Clamp (conteoRayosHorizontales, 2, int.MaxValue);
        //conteoRayosVerticales = Mathf.Clamp (conteoRayosVerticales, 2, int.MaxValue);

        espacioRayoHorizontal = limites.size.y / (conteoRayosHorizontales - 1);
		espacioRayoVertical = limites.size.x / (conteoRayosVerticales - 1);
	}

	//Episodio1
	public struct OrigenRayos{
		public Vector2 superiorIzquierda, superiorDerecha;
		public Vector2 inferiorIzquierda, inferiorDerecha;
	}
}
