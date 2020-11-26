using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controlador2D))]
public class Jugador : MonoBehaviour {					
	//Ep3 con estas2 variables se determina la velocidad en el eje Y y la gravedad
														
	public float maxAlturaSalto=4; //Ep10 renombradoalturaSalto		
    public float minAlturaSalto = 1;
    public float TiempoDeSaltoALaCima=.4f;
    float tiempoAceleracionVuelo = .2f;
    float tiempoAceleracionSuelo = .1f;
    [SerializeField]
    float velocidadMovimiento= 6; //Oculto	

    //Ep9
    public Vector2 paredsaltoEscalada;//x7.5 y16
    public Vector2 paredSaltoFuera;//x8.5 y7
    public Vector2 paredSalto;//x18 y17

    public float lisarEnParedVelMax=3;
    public float tiempoPegadoPared=.25f;
    float tiempoParaDespegarPared;
    
    //Ep3
	float gravedad ;// oculto = -20 aceleración
	float maxVelocidadSalto;//=8 velocidad final //Ep10 renombrado velocidadSalto
    float minVelocidadSalto;
    Vector3 velocidad;
    float velocidadXSuavizado;//es pasada por ref	

	Controlador2D controlador;

    Vector2 entradaDireccional;

    int paredDirX;
    bool lisarEnPared;

    void Start () {
		//Ep1
		controlador = GetComponent<Controlador2D> ();
		//Ep3 ecuaciones
		gravedad = -(2 * maxAlturaSalto) / Mathf.Pow (TiempoDeSaltoALaCima, 2);//Formula: Movimiento delta
        maxVelocidadSalto = Mathf.Abs (gravedad) * TiempoDeSaltoALaCima; //Formula: velocidad final
        minVelocidadSalto = Mathf.Sqrt(2 * Mathf.Abs(gravedad) * minAlturaSalto);//Formula: ecuacion kinematica sino estoy mal
		//print ("Gravedad: " + gravedad + " velocidad salto: " + velocidadSalto);
	}
	void Update(){
        //Vector2 entradaDireccional = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); Movido a EntradaJugador.cs Ep12
        CalcularVelocidad();
        ManejadorDeslizPorPared();
        
        #region movido 
        //Ep3 controla la gravedad acumulada //movida abajo Ep10 razones abajo
        //if (controlador.colisiones.arriba || controlador.colisiones.abajo)
        //{
        //    velocidad.y = 0;
        //}

        //Ep2
        //Vector2 entrada= new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));// movido arriba en Ep9

        //Ep3//Movido en Ep12
        //if (Input.GetKeyDown (KeyCode.Space)/* && controlador.colisiones.abajo//Cambiado en Ep9*/) {
        //if (lisarEnPared)
        //{
        //    //primer caso el ClimbJump o salto escalando
        //    if(paredDirX== entradaDireccional.x)
        //    {
        //        //nos moveremos cara a la pared 
        //        velocidad.x = -paredDirX * paredsaltoEscalada.x;
        //        velocidad.y = paredsaltoEscalada.y;
        //    }
        //    else if(entradaDireccional.x==0)
        //    {
        //        //cuando saltamos fuera de la pared
        //        velocidad.x = -paredDirX * paredSaltoFuera.x;
        //        velocidad.y = paredSaltoFuera.y;
        //    }
        //    else
        //    {
        //        //cuando saltamos opuesto a la pared
        //        velocidad.x = -paredDirX * paredSalto.x;
        //        velocidad.y = paredSalto.y;
        //    }
        //}
        ////Si esta en piso salte
        //if (controlador.colisiones.abajo)
        //velocidad.y = maxVelocidadSalto;
        //}
        //if (Input.GetKeyUp(KeyCode.Space))
        //{   //Aseguramos estar mas arriba del valor minimo de salto, evitamos interrumpir en medio del salto una caida inmediata
        //    if(velocidad.y > minVelocidadSalto)
        //        velocidad.y = minVelocidadSalto;
        //}

        //Ep3 suavizado en el cambio de direccion en el aire y el suelo
        //velocidad.x = entrada.x * velocidadMovimiento;//ep2
        //Ep9 Debemos calcularlo de nuevo arriba porque no tendria efecto de pegado a la pared si lo dejo aca
        //float objetivoVelocidadX = entrada.x * velocidadMovimiento;
        //velocidad.x = Mathf.SmoothDamp (velocidad.x, objetivoVelocidadX, ref velocidadXSuavizado,(controlador.colisiones.abajo)? tiempoAceleracionSuelo:tiempoAceleracionVuelo);
        #endregion Movidomovido
        //Ep2
        controlador.Movimiento (velocidad * Time.deltaTime,entradaDireccional);

        //Ep3 controla la gravedad acumulada //Ep10 movido aca porque lav velocidad en eje Y es reseteada si hay colision arriba o abajo
        //la razon es que si estamos sobre una plataforma  de movimiento la plataforma mobil tambien llama el controlador.Movimiento(arriba) que potencialmente altera
        //los valores de arriba y abajo asi que nos aseguramos que los valores se presenten despues de nuestro llamado al contro..con el Input que estamos haciendo
		if (controlador.colisiones.arriba || controlador.colisiones.abajo) {
            //Ep13 creo esto para que caiga mas rapido en pendientes mas empinadas
            if (controlador.colisiones.maxDeslicePendiente)//le quito ! mas adelante
                //velocidad.y = 0; //ep13
                velocidad.y += controlador.colisiones.pendienteNormal.y * -gravedad * Time.deltaTime;
            else
                velocidad.y = 0;

        }
	}

    public void EstablecerEntradaDireccional(Vector2 entrada)
    {
        entradaDireccional=entrada;
    }
    public void AlSaltarEntradaAbajo()
    {
        if (lisarEnPared)
        {
            //primer caso el ClimbJump o salto escalando
            if (paredDirX == entradaDireccional.x)
            {
                //nos moveremos cara a la pared 
                velocidad.x = -paredDirX * paredsaltoEscalada.x;
                velocidad.y = paredsaltoEscalada.y;
            }
            else if (entradaDireccional.x == 0)
            {
                //cuando saltamos fuera de la pared
                velocidad.x = -paredDirX * paredSaltoFuera.x;
                velocidad.y = paredSaltoFuera.y;
            }
            else
            {
                //cuando saltamos opuesto a la pared
                velocidad.x = -paredDirX * paredSalto.x;
                velocidad.y = paredSalto.y;
            }
        }
        //Si esta en piso salte
        if (controlador.colisiones.abajo)
        {   //si esta deslizandoce por una pendiente muy empinada
            if (controlador.colisiones.maxDeslicePendiente)
            {
                if(entradaDireccional.x != -Mathf.Sign(controlador.colisiones.pendienteNormal.x))//not salte contra la pendiente maxima
                {
                    velocidad.y = maxVelocidadSalto * controlador.colisiones.pendienteNormal.y;//al salto añadale la direccion del normal
                    velocidad.x = maxVelocidadSalto * controlador.colisiones.pendienteNormal.x;
                }
            }
            else
            {
            velocidad.y = maxVelocidadSalto;

            }
        }
    }
    public void AlSaltarEntradaArriba()
    {
        //Aseguramos estar mas arriba del valor minimo de salto, evitamos interrumpir en medio del salto una caida inmediata
        if (velocidad.y > minVelocidadSalto)
            velocidad.y = minVelocidadSalto;
    }
    void CalcularVelocidad()//Ep12
    {
        
        //Ep9 movido aca
        float objetivoVelocidadX = entradaDireccional/*entrada*/.x * velocidadMovimiento;
        velocidad.x = Mathf.SmoothDamp(velocidad.x, objetivoVelocidadX, ref velocidadXSuavizado, (controlador.colisiones.abajo) ? tiempoAceleracionSuelo : tiempoAceleracionVuelo);
        //Ep2 
        velocidad.y += gravedad * Time.deltaTime;
    }
    void ManejadorDeslizPorPared()
    {
        //sera -1 si el collider esta contra la pared a la izquierda y 1 si a la derecha
        paredDirX = (controlador.colisiones.izquierda) ? -1 : 1;
        //Ep9
        lisarEnPared = false;
        if ((controlador.colisiones.izquierda || controlador.colisiones.derecha) && !controlador.colisiones.abajo && velocidad.y < 0)
        {
            lisarEnPared = true;
            //si esta cayendo entre y resetee dicha velocidad
            if (velocidad.y < -lisarEnParedVelMax)
            {
                velocidad.y = -lisarEnParedVelMax;
            }
            //mientras haya tiempo para mantenerme pegado 
            if (tiempoParaDespegarPared > 0)
            {
                //evite que pueda dar direccion opuesta mientras nos mantenemos pegados
                velocidadXSuavizado = 0f;//tambien reseteamos este de la funcion SmoothDamp porque daria un resultado extraño
                velocidad.x = 0;

                //si estando pegado a la pared presiono la direccion contraria empieza la cuenta regresiva
                if (entradaDireccional.x != paredDirX && entradaDireccional.x != 0)
                    tiempoParaDespegarPared -= Time.deltaTime;
                else //si no otra vez tenemos tiempo
                    tiempoParaDespegarPared = tiempoPegadoPared;
            }
            else//si ya se acabo el tiempo restablezca y permita input en X
            {
                tiempoParaDespegarPared = tiempoPegadoPared;
            }
        }
    }

}
/*Problemas resueltos
 * evita la fuerza de gravedad acumulada
 * Salto adecuado por unidades y tiempo de salto
 * Cambio de direccion suavizado, realistico y salto tambien
 * Evitar resbalar en pendientes*/
 