using System.Collections;
using System.Collections.Generic;//lo añaden en Ep6 para usar los hashset
using UnityEngine;
//Creado en Ep6 es una extension mas de RaycastControlador ahora el Controlador2D y este script tienen la posibilidad de tener el generador de rayos sin necesidad
//de crear codidgo para cada uno
public class PlataformaControlador : RaycastControlador
{

    public LayerMask mascaraPasajera;
    // public Vector3 movimiento;//borrado en Ep8 reemplazado por funcion CalculatePlattformMovement para mover plataformas entre puntos

    //Ep8 
    public Vector3[] puntosDeRutaLocales;//posiciones relativas a la plataforma
    public Vector3[] puntosDeRutaGlobales;//se crea para que alamcene las posiciones globales y no se mueva con la plataforma

    public float velocidad;
    public bool ciclo;
    public float tiempoEspera;
    [Range(0,2)]//que en realidad seria hasta 3 pero mas de ahi sabemos que no da buen resultado
    public float cantidadAlivio;

    int desdePuntoDeRutIndice;
    float porcentajeEntrePuntosDeRuta;
    float proximoTiempoMovimiento;

    //Ep7
    List<MovimientoPasajero> movimientoPasajero;
    //Ahora crearemos un diccionario para reducir el numero de llamadas de getcomponent controlador2D ya que es un fallo de rendimiento considerable
    //Transform es la llave y el controller2D es el valor
    Dictionary<Transform, Controlador2D> pasajeroDiccionario = new Dictionary<Transform, Controlador2D>();

    //Ep6
    public override void Start()
    {
        base.Start();

        //Ep8 alamcena las posiciones locales para que no muevan con la plataforma
        puntosDeRutaGlobales = new Vector3[puntosDeRutaLocales.Length];
        for (int i = 0; i < puntosDeRutaLocales.Length; i++)
        {
            puntosDeRutaGlobales[i] = puntosDeRutaLocales[i] + transform.position;
        }
    }


    void Update()
    {
        ActualizarOrigenRayos();

        Vector3 velocidad = CalcularMovimientoPlataforma()/*movimiento * Time.deltaTime*/;//Ep8 reemplazado

        CalcularMovimientoPasajero(velocidad);

        //Una vez se haya movido la plataforma
        MovimientoPasajeros(true);
        transform.Translate(velocidad);
        MovimientoPasajeros(false);
    }

    float Alivio(float x)
    {
        float a = cantidadAlivio + 1;//Se supone que cuando ponemos 0 esperamos no tener alivio pero la formula requiere 1 para eso
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x,a));
    }

    Vector3 CalcularMovimientoPlataforma()
    {
        if (Time.time < proximoTiempoMovimiento)
        {
            return Vector3.zero;
        }
        
        //Ep8 esto lo reseteara a cero una vez atraviese todo el tamaño
        desdePuntoDeRutIndice %= puntosDeRutaGlobales.Length;
        //print("0 % 3: " + 0 % 3);//=0 
        //print("1 % 3: "+1 % 3);//=1
        //print("2 % 3: " + 2 % 3);//=2 formula calculadora basica: Cociente 2/3= 0 para encontrar Residuo 2-3*0= 2
        //print("3 % 3: " + 3 % 3);//=0 fuera del array pero sirve para reiniciar valores y actuar en ciclo
        //Ep7
        int haciaPuntoDeRutaIndice = (desdePuntoDeRutIndice + 1)/*Ep8-> cuando desde es 2 hacia es 3 pero se convierte en cero y genera ciclo*/ % puntosDeRutaGlobales.Length;
        float distanciaEntrePuntosDeRuta = Vector3.Distance(puntosDeRutaGlobales[desdePuntoDeRutIndice], puntosDeRutaGlobales[haciaPuntoDeRutaIndice]);
        //Lo dividimos entre la distancia ya que el incrementa a un ratio constante y si la plataforma esta lejos aumenta drasticamente la velocidad
        porcentajeEntrePuntosDeRuta += Time.deltaTime * velocidad / distanciaEntrePuntosDeRuta;
        //sujetamos porcentaje para evitar comportamientos extraños en la funcion de alivio
        porcentajeEntrePuntosDeRuta = Mathf.Clamp01(porcentajeEntrePuntosDeRuta);
        float alivioPorcentajeEntrePuntosDeRuta = Alivio(porcentajeEntrePuntosDeRuta);//esto se usara en el lerp perfectamente modificado y estilizado

        //lerpiamos segun el porcentaje
        Vector3 nuevaPosicion = Vector3.Lerp(puntosDeRutaGlobales[desdePuntoDeRutIndice], puntosDeRutaGlobales[haciaPuntoDeRutaIndice], alivioPorcentajeEntrePuntosDeRuta);

        //si alcanza el 100% reseteamos el porcentaje y aumentamos el siguiente punto de destino
        if (porcentajeEntrePuntosDeRuta >= 1)
        {
            porcentajeEntrePuntosDeRuta = 0;
            desdePuntoDeRutIndice++;
            //si no esta en ciclo
            if (!ciclo)
            {
                //si alcanza el limite del array reinicie desde cero
                if (desdePuntoDeRutIndice >= puntosDeRutaGlobales.Length - 1)
                {
                    desdePuntoDeRutIndice = 0;
                    //si queremos que se regrese los puntos y no reiniciarlos podemos usar 
                    System.Array.Reverse(puntosDeRutaGlobales);
                }
            }
            proximoTiempoMovimiento = Time.time + tiempoEspera;
        }
       
        return nuevaPosicion - transform.position;
    }

    //Ep7
    void MovimientoPasajeros(bool movimientoAntesPlataforma)
    {
        //a una variable pasajero de tipo Struct llevele la lista recolectada de movimientoPasajero que contiene la info y que debe hacer primero el pasajero
        foreach (MovimientoPasajero pasajero in movimientoPasajero)
        {
            //Ep7 Si el diccionario no tiene esa llave "transform" por primera vez ingresada entonces entre 
            if (!pasajeroDiccionario.ContainsKey(pasajero.transform))
            {
                //añada por primera y uncia vez al diccionario ese transform y su script Controlador2D y evite que vuelva a entrar por ser ciclo
                pasajeroDiccionario.Add(pasajero.transform, pasajero.transform.GetComponent<Controlador2D>());
            }
            //lo contenido en la nueva lista generada por el ciclo lo comparamos con la variable de entrada de este metodo que en un frame se prende y se apaga luego (mas arriba de este codigo)
            if (pasajero.moverAntesPlataforma == movimientoAntesPlataforma)
            {
                //Aca le decimos que obtenga el objeto de esa lista y acceda al controlador2D y al metodo de movimiento le lleve la velocidad elegida por este script
                //fue editada en el Ep7 para mas obtimización con diccionarios
                //pasajero.transform.GetComponent<Controlador2D> ().Movimiento (pasajero.velocidad,pasajero.paradoSobrePlataforma);
                //En el diccionario en esa posicion dela lista del ciclo osea en el primer registro del diccionario ejecute el metodo movimiento y asi nos aseguramos que solo sea llamado una vez el componente por pasajero
                pasajeroDiccionario[pasajero.transform].Movimiento(pasajero.velocidad, pasajero.paradoSobrePlataforma);

            }
        }
    }

    //Ep6 MovimientoPasajero y en Ep7 cambia el nombre:
    void CalcularMovimientoPasajero(Vector3 velocidad)
    {
        //notece la s añadida en movimientoS, 
        //en este caso este objeto Hashset almacenara el transform de los pasajeros detectados en la plataforma en ese frame 
        //y se hace porque el rayo emitido podria detectar vairas veces el mismo pasajero generando conflicto
        //los hashset son utilizados porque son particularmente rapidos para añadir cosas y comprobar si contienen ciertas cosas
        // para usarlos necesitamos using System.Collections.Generic;
        HashSet<Transform> movimientosPasajeros = new HashSet<Transform>();
        //Ep7 inicializamos la lista de tipo struct MovimientoPasajero
        movimientoPasajero = new List<MovimientoPasajero>();

        float direccionX = Mathf.Sign(velocidad.x);
        float direccionY = Mathf.Sign(velocidad.y);

        //Movimiento Vertical de la plataforma
        if (velocidad.y != 0)
        {
            float longitudRayo = Mathf.Abs(velocidad.y) + anchoPiel;

            for (int i = 0; i < conteoRayosVerticales; i++)
            {
                Vector2 origenRayo = (direccionY == -1) ? origenRayos.inferiorIzquierda : origenRayos.superiorIzquierda;
                origenRayo += Vector2.right * (espacioRayoVertical * i);
                RaycastHit2D golpe = Physics2D.Raycast(origenRayo, Vector2.up * direccionY, longitudRayo, mascaraPasajera);
                //Debug.DrawRay (origenRayos.inferiorIzquierda + Vector2.right * espacioRayoVertical * i, Vector2.up * -2, Color.red);
                Debug.DrawRay(origenRayo, Vector2.up * direccionY * longitudRayo, Color.cyan);
                //Si encuentra un pasajero
                if (golpe && golpe.distance !=0)//Ep10 añadimos validacion de distance para que no tenga influencia en el jugador con una plataforma mobil al atravesarla
                {
                    //Cada vez que golpe encuentre algo, Si el objeto HashSet no contiene algo que golpeo osea un pasajero solo entonces movera a ese pasajero o ese transform
                    // y procedera a almacenarlo en el hashset
                    if (!movimientosPasajeros.Contains(golpe.transform))
                    {
                        //añadimos al hashset el pasajero y esto permite que el pasajero sea detectado y movido una vez por frame y no mas veces por ciclo for
                        movimientosPasajeros.Add(golpe.transform);
                        float empujarX = (direccionY == 1) ? velocidad.x : 0;
                        float empujarY = velocidad.y - (golpe.distance - anchoPiel) * direccionY;
                        // añadimos a la lista de tipo struct la informacion y como va hacia arriba queremos que se mueva primero el pasajero 
                        //si esta parado sobre la plataforma true sino false porque los rayos se emitiran hacia abajo
                        movimientoPasajero.Add(new MovimientoPasajero(golpe.transform, new Vector3(empujarX, empujarY), direccionY == 1, true));
                        //Eliminado en Ep7
                        //golpe.transform.Translate (new Vector3 (empujarX, empujarY));
                    }
                }
            }
        }

        //Movimiento Horizontal de la plataforma
        //Si se esta moviendo de lado puede empujar al jugador
        if (velocidad.x != 0)
        {

            float longitudRayo = Mathf.Abs(velocidad.x) + anchoPiel;

            for (int i = 0; i < conteoRayosHorizontales; i++)
            {
                Vector2 origenRayo = (direccionX == -1) ? origenRayos.inferiorIzquierda : origenRayos.inferiorDerecha;
                origenRayo += Vector2.up * (espacioRayoHorizontal * i);//aca se omite VELOCITY.X algo de codigo que en vertical no pero es para que no tiemble el jugador
                RaycastHit2D golpe = Physics2D.Raycast(origenRayo, Vector2.right * direccionX, longitudRayo, mascaraPasajera);
                //Debug.DrawRay (origenRayos.inferiorIzquierda + Vector2.right * espacioRayoVertical * i, Vector2.up * -2, Color.red);
                Debug.DrawRay(origenRayo, Vector2.right * direccionX * longitudRayo, Color.cyan);
                if (golpe && golpe.distance != 0)
                {
                    if (!movimientosPasajeros.Contains(golpe.transform))
                    {
                        movimientosPasajeros.Add(golpe.transform);
                        float empujarX = velocidad.x - (golpe.distance - anchoPiel) * direccionX;
                        float empujarY = -anchoPiel;//Ep7 cambiamos el valor de 0 para ejercer un poco de gravedad y el pueda saltar al poder detectar piso
                                                    //aca el pasajero es empujado desde el lado por lo tanto no esta sobre la plataforma y por esto se pone false y queremos que el pasajero reaccione antes
                        movimientoPasajero.Add(new MovimientoPasajero(golpe.transform, new Vector3(empujarX, empujarY), false, true));

                        //Eliminado en Ep7: golpe.transform.Translate (new Vector3 (empujarX, empujarY));
                    }
                }
            }
        }

        //Tercer caso Pasajero encima: Si plataforma moviendoce hacia abajo o horizontalmente 
        if (direccionY == -1 || velocidad.y == 0 && velocidad.x != 0)
        {
            //se multiplica por 2 un skinwidth para obtener la superficie y otro para tener un pequeño rayo que detecte cualquier cosa encima
            float longitudRayo = anchoPiel * 2;

            for (int i = 0; i < conteoRayosVerticales; i++)
            {
                //Siempre queremos que emita los rayos desde topleft hacia right
                Vector2 origenRayo = origenRayos.superiorIzquierda + Vector2.right * (espacioRayoVertical * i);
                RaycastHit2D golpe = Physics2D.Raycast(origenRayo, Vector2.up, longitudRayo, mascaraPasajera);
                //Debug.DrawRay (origenRayos.inferiorIzquierda + Vector2.right * espacioRayoVertical * i, Vector2.up * -2, Color.red);
                Debug.DrawRay(origenRayo, Vector2.up * direccionY * longitudRayo, Color.cyan);
                if (golpe && golpe.distance != 0)
                {
                    if (!movimientosPasajeros.Contains(golpe.transform))
                    {
                        movimientosPasajeros.Add(golpe.transform);
                        float empujarX = velocidad.x;
                        float empujarY = velocidad.y;
                        //Ep7 como va hacia abajo la plataforma entonces necesitamos que se mueva primero que el pasajero entonces ponemos false
                        //añadimos a la lista de tipo struct los datos a la funcion de inicializacion
                        movimientoPasajero.Add(new MovimientoPasajero(golpe.transform, new Vector3(empujarX, empujarY), true, false));
                        //Eliminado en Ep7: golpe.transform.Translate (new Vector3 (empujarX, empujarY));
                    }
                }
            }
        }
    }

    struct MovimientoPasajero
    {
        public Transform transform;
        public Vector3 velocidad;
        public bool paradoSobrePlataforma;
        //cuando deberia o no moverse el pasajero antes que la plataforma
        public bool moverAntesPlataforma;

        public MovimientoPasajero(Transform _transform, Vector3 _velocidad, bool _paradoSobrePlataforma, bool _moverAntesPlataforma)
        {
            transform = _transform;
            velocidad = _velocidad;
            paradoSobrePlataforma = _paradoSobrePlataforma;
            moverAntesPlataforma = _moverAntesPlataforma;
        }
    }

    private void OnDrawGizmos()
    {   //si no esta vaicio
        if (puntosDeRutaLocales != null)
        {
            Gizmos.color = Color.red;
            float tamaño = .3f;
            //posicion de cada uno
            for (int i = 0; i < puntosDeRutaLocales.Length; i++)
            {
                //necesitamos convertir la posicion local en una posicion global para dibujar el gizmo
                //le añadimos aplication.isp.. para que los puntos se mantenga fijos cuando esta corriendo y sino las cruces se muevan con la plataforma
                Vector3 posGlobalPuntoDeRuta = (Application.isPlaying) ? puntosDeRutaGlobales[i] : puntosDeRutaLocales[i] + transform.position;
                //ahora queremos dibujar el gizmo en el centro
                //dibujamos una cruz primero la linea vertical y luego horizontal
                Gizmos.DrawLine(posGlobalPuntoDeRuta - Vector3.up * tamaño, posGlobalPuntoDeRuta + Vector3.up * tamaño);
                Gizmos.DrawLine(posGlobalPuntoDeRuta - Vector3.left * tamaño, posGlobalPuntoDeRuta + Vector3.left * tamaño);
            }
        }
    }
}

