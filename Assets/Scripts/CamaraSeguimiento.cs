using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Ep11
public class CamaraSeguimiento : MonoBehaviour
{
    public Controlador2D objetivo;
    public float compensacionVertical;
    public float miradaAdelanteDisX;
    public float miradaSuavidadTiempoX;
    public float tiempoSuavizadoVertical;
    public Vector2 tamañoAreaEnfoque;

    AreaEnfoque areaEnfoque;

    float actualMiradaAdelanteX;
    float objetivoMiradaAdelanteX;
    float dirMiradaAdelanteX;
    float suavizadoVelMiradaX;
    float velocidadSuavizadoY;

    bool detenerMiradaAdelante;

    void Start()
    {
        areaEnfoque = new AreaEnfoque(objetivo.colisionador.bounds, tamañoAreaEnfoque);//aca puedo ver la herencia en su expresion para objetivo
    }
    //Util para scripts de seguimiento de camara lo que significa que todo el movimiento del jugador ya ha terminado el frame en nuestro propio metodo Actualizar mas abajo
    void LateUpdate()
    {
        areaEnfoque.Actualizar(objetivo.colisionador.bounds);
        Vector2 posicionEnfoque = areaEnfoque.centro + Vector2.up * compensacionVertical;

        if (areaEnfoque.velocidad.x != 0)
        {
            dirMiradaAdelanteX = Mathf.Sign(areaEnfoque.velocidad.x);
            //evita que la camara se vaya de lado a lado incluso cuando damos un pequeño input opuesto exagerando la mirada espaciada que queremos enfrente del jugador
            if (Mathf.Sign(objetivo.entradaJugador.x)== Mathf.Sign(areaEnfoque.velocidad.x) && objetivo.entradaJugador.x != 0)//recuerde que el signo de cero es 1 (pregunta que tenia hace rato) por eso se añade mas validacion
            {
                detenerMiradaAdelante = false;
                objetivoMiradaAdelanteX = dirMiradaAdelanteX * miradaAdelanteDisX;
            }else//queremos que frene la camara pero no muy abrupto
            {
                if (!detenerMiradaAdelante)
                    {
                        detenerMiradaAdelante = true;
                        objetivoMiradaAdelanteX = actualMiradaAdelanteX + (dirMiradaAdelanteX * miradaAdelanteDisX - actualMiradaAdelanteX)/4f;//que tan lejos queremos que vaya
                    }
            }
        }

        //objetivoMiradaAdelanteX = dirMiradaAdelanteX * miradaAdelanteDisX; movido arriba mas adelante en el video
        actualMiradaAdelanteX = Mathf.SmoothDamp(actualMiradaAdelanteX, objetivoMiradaAdelanteX, ref suavizadoVelMiradaX, miradaSuavidadTiempoX);

        posicionEnfoque.y = Mathf.SmoothDamp(transform.position.y, posicionEnfoque.y, ref velocidadSuavizadoY, tiempoSuavizadoVertical);
        posicionEnfoque += Vector2.right * actualMiradaAdelanteX;

        transform.position = (Vector3)posicionEnfoque + Vector3.forward * -10;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .5f);
        Gizmos.DrawCube(areaEnfoque.centro, tamañoAreaEnfoque);
    }

    struct AreaEnfoque
    {
        public Vector2 centro;
        public Vector2 velocidad;// para saber cuanto se desplazo el area de enfoque en el ultimo frames
        float izquierda, derecha;
        float superior, inferior;

        
        public AreaEnfoque(Bounds obejtivoLimites, Vector2 tamaño)
        {
            izquierda = obejtivoLimites.center.x - tamaño.x / 2;
            derecha = obejtivoLimites.center.x + tamaño.x / 2;
            inferior = obejtivoLimites.min.y;
            superior = obejtivoLimites.min.y + tamaño.y;

            velocidad = Vector2.zero;
            centro = new Vector2((izquierda + derecha) / 2, (superior + inferior) / 2);
        }

        public void Actualizar(Bounds objetivoLimites)
        {
            float desplazarX=0;
            if (objetivoLimites.min.x < izquierda)
            {
                desplazarX = objetivoLimites.min.x - izquierda;
            }
            else if (objetivoLimites.max.x > derecha)
            {
                desplazarX = objetivoLimites.max.x - derecha;
            }
            izquierda += desplazarX;
            derecha += desplazarX;

            float desplazarY = 0;
            if (objetivoLimites.min.y < inferior)
            {
                desplazarY = objetivoLimites.min.y - inferior;
            }
            else if (objetivoLimites.max.y > superior)
            {
                desplazarY = objetivoLimites.max.y - superior;
            }
            superior += desplazarY;
            inferior += desplazarY;
            centro = new Vector2((izquierda + derecha) / 2, (superior + inferior) / 2);
            velocidad = new Vector2(desplazarX, desplazarY);
        }

    }
}
