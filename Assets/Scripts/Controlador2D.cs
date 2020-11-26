using UnityEngine;
using System.Collections;
// movido a RaycastControlador Script en el Ep6
//[RequireComponent(typeof(BoxCollider2D))]
//Pasa a heredar de MonoBehaviour a RaycastControlador Ep6
public class Controlador2D : RaycastControlador {
    //Ep13
    //public float maxAnguloAscenso = 80;
    //public float maxAnguloDescenso = 75;
    public float maxAngulo = 80;

    /* movido a RaycastControlador Script en el Ep6
    public LayerMask mascaraColision;

	const float anchoPiel=.015f;//const 015f
	public int conteoRayosHorizontales=4;
	public int conteoRayosVerticales=4;

	float espacioRayoHorizontal;
	float espacioRayoVertical;

	[Header("Pruebas")]
	//public float distanciaMovimiento;
	//public float cantidadMovimientoAscensoY;

	BoxCollider2D colisionador;
	OrigenRayos origenRayos;
   */

    public ColisionInfo colisiones;
    [HideInInspector]
    public Vector2 entradaJugador;//Ep10
    
    /*movido a RaycastControlador Script en el Ep6
     void Start(){
		colisionador = GetComponent<BoxCollider2D> ();
		CalcularEspacioRayos ();
	}*/
    public override void Start(){
		base.Start ();
        colisiones.caraDir = 1;//Ep9
	}
    /*void Update(){
		Episodio1
		 movido a metodo movimiento ep2 ActualizarOrigenRayos ();
		 movido al start en ep2 CalcularEspacioRayos ();
		for(int i=0;i<conteoRayosVerticales;i++){
			Debug.DrawRay (origenRayos.inferiorIzquierda + Vector2.right * espacioRayoVertical * i, Vector2.up * -2, Color.red);
		}
	}*/
    //Ep10 creamos un metodo sobrecargado ya que la clase Plataforma accede tambien a este metodo y no queremos estar especificando el input asi dicha clase no tiene que preocuparse
    public void Movimiento(Vector2 cantidadMovimiento, bool paradoEnPlataforma)//Ep12 refactorizamos todos los vectore3 por Vectores2 ya que no usamos el eje z y el nombre cantidadMovimiento porque aca recibimos es la cantidad de movimiento al estar ya multiplicado por deltatime
    {
        Movimiento(cantidadMovimiento, Vector2.zero, paradoEnPlataforma);
    }
	
    //Episodio2, en Ep7 se añade paradoEnPlataforma para que pueda saltar sobre plataformas ya que no sabe que esta sobre una //Ep10 añade entrada
	public void Movimiento(Vector2 cantidadMovimiento, Vector2 entrada, bool paradoEnPlataforma=false){
		ActualizarOrigenRayos ();
		colisiones.Reset ();//Ep3
        colisiones.cantidadMovimientoAntigua=cantidadMovimiento;//para angulos en V Ep5        
        entradaJugador = entrada;
        //Ep13 movido aca para poder deslizarse  sin que la direccion generada debajo de este pueda interferir y sirve para deslizarse defrente a la pendiente si que se reduzca la velocidad
        if (cantidadMovimiento.y < 0)
        {
            BajarPendientes(ref cantidadMovimiento);
        }
        //Ep9
        if (cantidadMovimiento.x != 0)
        {
            colisiones.caraDir = (int)Mathf.Sign(cantidadMovimiento.x);
        }
		//if (cantidadMovimiento.y < 0) {//Ep13 movido arriba para que pueda deslizar por pendiente cuando esta defrente
		//	BajarPendientes (ref cantidadMovimiento);
		//}

		//Ep2
		//if (cantidadMovimiento.x != 0) {//removido en Ep9 para poder detectar el wallslide desde un salto sin input horizontal
		ColisionesHorizontales (ref cantidadMovimiento);
		//}
		if (cantidadMovimiento.y != 0) {
			ColisionesVerticales (ref cantidadMovimiento);
		}
		//Ep1
		transform.Translate (cantidadMovimiento);
        //Ep7 para que pueda saltar encima de plataformas mobiles
		if (paradoEnPlataforma) {
			colisiones.abajo = true;
		}
	}

	//Episodio2: colisiones derecha e izquierda
	void ColisionesHorizontales(ref Vector2 cantidadMovimiento){
		
		float direccionX = colisiones.caraDir/*Mathf.Sign (cantidadMovimiento.x)*/;//ep9 reemplazado
		float longitudRayo = Mathf.Abs (cantidadMovimiento.x) + anchoPiel;//este mueve el rayo al borde del collider
        //si me quedo quieto la cantidadMovimiento sera cero por lo que la longitud del rayo crecera
        if(Mathf.Abs(cantidadMovimiento.x) < anchoPiel)
        {
            longitudRayo = 2 * anchoPiel;//este sinembarbo añade un poco de distancia para detectar la pared y poder deslizarme
        }
		for(int i=0; i < conteoRayosHorizontales; i++){
			Vector2 origenRayo = (direccionX == -1 )? origenRayos.inferiorIzquierda : origenRayos.inferiorDerecha;
			origenRayo += Vector2.up * (espacioRayoHorizontal  * i );//aca se omite VELOCITY.X algo de codigo que en vertical no "pero es para que no tiemble el jugador porque se combinaria con gravedad"
            RaycastHit2D golpe = Physics2D.Raycast (origenRayo, Vector2.right * direccionX, longitudRayo, mascaraColision);
			//Debug.DrawRay (origenRayos.inferiorIzquierda + Vector2.right * espacioRayoVertical * i, Vector2.up * -2, Color.red);
			Debug.DrawRay (origenRayo,Vector2.right * direccionX /** longitudRayo*/, Color.red);

			//Si golpea algo horizontalmente
			if (golpe) {
                //Ep7 para evitar movimiento indeseado cuando una plataforma atraviesa al jugador de arriba a abajo como aplastandolo y Tratamos de movernos
                /*se genera un movimiento opuesto al input que lo ocasiona esta linea mas abajo: cantidadMovimiento.x = (golpe.distance - anchoPiel) * direccionX; 
                 * que se supone es colision contra una pared; con esto le decimos vaya directamente al siguiente rayo y uselo para determinar las colisiones horziontales
                 * ignorando el actual y si la plataforma esta bajando entonces supongo la atraviesa y ya no la puede contrarestar*/
                if (golpe.distance == 0)
                {
                    continue;
                }

                //Ep4 encontramos el angulo de las pendientes
                float anguloInclinacion = Vector2.Angle (golpe.normal, Vector2.up);
				//Pendiente: si el rayo es el mas inferior o primero de abajo hacia arriba y el angulo es menor al maximo permitido
				if (i == 0 && anguloInclinacion <= maxAngulo) {

					//EP5 Evita que se ralentice cuando se desciende y de repente esta ascendiendo como en un angulo de V.
					if(colisiones.bajandoPendiente){
						colisiones.bajandoPendiente = false;
                        cantidadMovimiento = colisiones.cantidadMovimientoAntigua;
                    }
					//Ep4 Evita que levite en la pendiente por deteccion anticipada
					float distanciaInicioPendiente=0;
					//En otras palabras si comienza a escalar una pendiente nueva
					if (anguloInclinacion != colisiones.anguloInclinaciónAntiguo) {
						distanciaInicioPendiente = golpe.distance - anchoPiel;
						//Restele ese pequeño tramo
						cantidadMovimiento.x -= distanciaInicioPendiente * direccionX;
					}
					//Ep4 Metodo subir pendientes
					SubirPendiente (ref cantidadMovimiento, anguloInclinacion, golpe.normal);
					//una vez deje de subir pendientes normalice la pequeña distancia que habia sustraido
					cantidadMovimiento.x += distanciaInicioPendiente * direccionX;

				}
				//Ep4 la unica manera de que queremos comprobar el resto de rayos es
				//(PLano:) Si no esta subiendo pendientes osea golpeando una pared o una pared inclinada
				if (!colisiones.subiendoPendiente || anguloInclinacion > maxAngulo) {//ayuda tambien a que no se ejecuta varias veces el Chequeo del resto de rayos
					//Ep 2 reducimos la cantidadMovimiento de x a 0 porque golpeo con algo
					cantidadMovimiento.x = (golpe.distance - anchoPiel) * direccionX;
					longitudRayo = golpe.distance;
					//Pendiente: EP4 evita movimiento indeseado al entrar en contacto con muro en pendiente porque debemos actualizar Y ya que X esta en cero pero Y no por la subida de pendiente
					if (colisiones.subiendoPendiente) {
                        //Usamos tangente para averiguar de nuevo el cateto opuesto y recalculamos cantidadMovimiento en Y
                        //Ya no sabemos moveDistance "hipotenusa" (porque estamos en 0 detenidos en X por la pared) entonces si no hay hipotenusa no podemos usar seno (porque serian dos datos desconocidos) 
                        //ni coseno (porque no se relaciona con opuesto que es el que necesito) 
                        // video de explicacion: https://www.youtube.com/watch?v=WASMUsVHFXk&ab_channel=AcademiaInternet
                        //ya que queremos conocer opuesto y tenemos angulo y valor del adyacente entonces usamos tangete porque se relaciona: tan(angulo) = opuesto (desconozco) y Adyacente(lo conozco es cantidadMovimiento.x)
                        // y nos aseguramos que sea colisi.anguloincli porque el anguloincli se actualiza con cada rayo y necesitamos solo el del subir pendientes
                        cantidadMovimiento.y = Mathf.Tan (colisiones.anguloInclinacion * Mathf.Deg2Rad) * Mathf.Abs (cantidadMovimiento.x);
					}
					//Ep3 establece a true la colision segun las direcciones izquierda y derecha si golpea algo
					colisiones.izquierda = direccionX == -1;
					colisiones.derecha = direccionX == 1;
				}
			} 
		}
	}
	//Episodio2 añadido del metodo update del ep1
	//colisiones arriba y abajo
	void ColisionesVerticales(ref Vector2 cantidadMovimiento){
		//Encuentra la direccion basado en la gravedad
		float direccionY = Mathf.Sign (cantidadMovimiento.y);
		//Se le suma cantidadMovimiento para que el tamaño del rayo llegue a tiempo con el collider
		float longitudRayo = Mathf.Abs (cantidadMovimiento.y) +  anchoPiel;

		for(int i=0; i < conteoRayosVerticales; i++){
			Vector2 origenRayo = (direccionY == -1 )? origenRayos.inferiorIzquierda : origenRayos.superiorIzquierda;
			origenRayo += Vector2.right * (espacioRayoVertical * i + cantidadMovimiento.x);
			RaycastHit2D golpe = Physics2D.Raycast (origenRayo, Vector2.up * direccionY, longitudRayo, mascaraColision);
			//Debug.DrawRay (origenRayos.inferiorIzquierda + Vector2.right * espacioRayoVertical * i, Vector2.up * -2, Color.red);
			Debug.DrawRay (origenRayo,Vector2.up * direccionY /** longitudRayo*/, Color.red);
			//print (cantidadMovimiento.x);
			if (golpe) {
                //Ep10 si golpea una plataforma atravesable o si la distancia es igual a cero (esto valida que no atraviese al final la plataforma
                //en camara lenta por el ancho de piel aplicado abajo aunque lo testie y sigue sucediendo, Bug:atravesar esto en pendientes solo funciona si es bajada 
                if (golpe.collider.tag == "Atravesar" ||  golpe.distance ==0)
                {   //y vas direccion arriba
                    if (direccionY == 1)
                    {   //pasa al siguiente rayo saltando el codigo abajo 
                        continue;
                    }
                    //reforzamos la caida atraves de la plataforma hecha abajo
                    if (colisiones.caerAtravesPlataforma)
                    {
                        continue;
                    }
                    //Si el jugador esta parado en una plataforma atravesable y presiona tecla abajo // y si esta cayendo uno puede alterar la cantidadMovimiento de caida presionando ysoltando lei
                    if (entradaJugador.y == -1)
                    {
                        colisiones.caerAtravesPlataforma = true;
                        Invoke("ResetearCaerAtravesPlataforma",.1f);//.5f lo cambie porque sino no me detecta la otra plataforma que puse debajo y sigue derecho
                        continue;
                    }
                }
                
                //Ep2
				//Como ya viene con la gravedad ejerciendo Ajustamos o reducimos la cantidadMovimiento para que no siga en caida libre quedaria en cero - la distancia dela piel y la direccion puesto que si toca techo ocurre lo opuesto
				cantidadMovimiento.y = (golpe.distance - anchoPiel) * direccionY;
                //la distancia de rayo es equivalente a lo primero que se encuentre y evita que traspace un obstaculo en el borde por ejemplo(del video) al encontrar un obstaculo 
                //mas arriba del suelo hace que los otros rayos tengan una nueva dimension mas corta y deja practicamente solo el skinwhidt de longitud
				longitudRayo = golpe.distance;
			
				//EP4 evita movimiento indeseado al entrar en contacto con techo en pendiente
				if (colisiones.subiendoPendiente) {
					//recalcula cantidadMovimiento en x
					cantidadMovimiento.x = cantidadMovimiento.y /Mathf.Tan (colisiones.anguloInclinacion * Mathf.Deg2Rad) * Mathf.Sign (cantidadMovimiento.x);
				}

				//Ep3 establece a true la colision segun las direcciones izquierda y derecha
				colisiones.abajo = direccionY == -1;
				colisiones.arriba = direccionY == 1;
            }
		}
		/*Ep5 tal vez no es necesaria si se mejora colisiones horizontales puesto que tiene que ver con el origenRayos por que le quito la suma de Y en 
         * colisiones horizontales pero si se lo pongo hace que vibra en pendientes cuando uno se queda quieto, entendible por la fuerza de la gravedad 
         * aplicada contra el hit.distance*/
		//Ep5 evita atascarse un poco cuano encuentra otra pendiente de mas o que se entierre en otra pendiente que haya cuando esta subiendo una
		if (colisiones.subiendoPendiente) {
			float direccionX = Mathf.Sign (cantidadMovimiento.x);
			longitudRayo = Mathf.Abs (cantidadMovimiento.x) + anchoPiel;
            //asi emitiremos desde una nueva altura dice el hombre (se emite un poco desde mas arriba lo que detecta rapidamente si entra una mas inclinada, igual a colisionHorizontal pero añade el final
            Vector2 origenRayo = ((direccionX == -1 )? origenRayos.inferiorIzquierda : origenRayos.inferiorDerecha) + Vector2.up * cantidadMovimiento.y;
			RaycastHit2D golpe = Physics2D.Raycast (origenRayo, Vector2.right * direccionX, longitudRayo, mascaraColision);
			Debug.DrawRay (origenRayo,Vector2.right * direccionX * longitudRayo, Color.blue);

			if (golpe) {
				float anguloInclinacion = Vector2.Angle (golpe.normal, Vector2.up);
				//Si el nuevo angulo es diferente al angulo del frame anterior es que tenemos una nueva pendiente
				if (anguloInclinacion != colisiones.anguloInclinacion) {
					cantidadMovimiento.x = (golpe.distance - anchoPiel) * direccionX;
					colisiones.anguloInclinacion = anguloInclinacion;
                    colisiones.pendienteNormal = golpe.normal;
                }
			}
		}
	}

	//Episodio4
	void SubirPendiente(ref Vector2 cantidadMovimiento,float anguloInclinacion , Vector2 pendienteNormal){
        //print(cantidadMovimiento);
        /*Queremos que la cantidadMovimiento de subida en la pendiente sea como si nos estuvieramos moviendo normal asi que la cantidadMovimiento de eje X la trataremos  
         *como la distancia total de la pendiente , usando la distancia de movimiento deseado y el angulo podemos configurar nuestra nueva cantidadMovimiento en X e Y
         en la imagen en blanco estan las variables que conocemos D entendido como MoveDistance o distanciaMovimiento y Theta entendido por el angulo de la pendiente
         y queremos encontrar la variables en rojo X e Y
         */
       
        //Equivale a la hipotenusa osea a toda la distancia de la pendiente
		float distanciaMovimiento = Mathf.Abs (cantidadMovimiento.x);//movimiento objetivo
		//Seno sirve para encontrar el cateto opuesto osea Y
		float cantidadMovimientoAscensoY= Mathf.Sin (anguloInclinacion * Mathf.Deg2Rad) * distanciaMovimiento;
        //Permite hacer saltos en pendientes correccion 2
        #region//if ( cantidadMovimiento.y > cantidadMovimientoAscensoY)
        //{
        //    print("Salto en pendiente");
        //} else
        #endregion
        //Impide que se autoresetee la cantidadMovimiento en Y al estar subiendo mientras ejecuto el salto en una pendiente por las colisiones horizontales 
        if (cantidadMovimiento.y <= cantidadMovimientoAscensoY) {
			//Mientras no salte mantenga la cantidadMovimiento de ascenso por la pendiente
			cantidadMovimiento.y = cantidadMovimientoAscensoY;
			//Permite subir pendientes a una cantidadMovimiento adecuada
			//Coseno sirve para encontar el cateto adyacente osea X porque conozco la hipotenusa y el angulo
			cantidadMovimiento.x = Mathf.Cos (anguloInclinacion * Mathf.Deg2Rad) * distanciaMovimiento * Mathf.Sign(cantidadMovimiento.x);
			//permite el salto en pendietes  correccion 1
			colisiones.abajo = true;
			colisiones.subiendoPendiente = true;
            //actualizo el angulo de inclinacion de colisiones encontradas
            colisiones.anguloInclinacion = anguloInclinacion;//Este ya no esta en Ep13 y yo lo tenia activo
            colisiones.pendienteNormal = pendienteNormal;
        }   
	}

	//Episodio 5
	 void BajarPendientes( ref Vector2 cantidadMovimiento){
        //Ep13
        RaycastHit2D maxGolpePendienteIzq = Physics2D.Raycast(origenRayos.inferiorIzquierda, Vector2.down, Mathf.Abs(cantidadMovimiento.y)+  anchoPiel, mascaraColision);
        RaycastHit2D maxGolpePendienteDer = Physics2D.Raycast(origenRayos.inferiorDerecha, Vector2.down, Mathf.Abs(cantidadMovimiento.y) + anchoPiel, mascaraColision);
        if (maxGolpePendienteIzq ^ maxGolpePendienteDer)//Operador logico exclusivo (acento circunflejo) se hace con Alt + 94 significa o uno o el otro pero solo uno de los dos
        {
            MaxDeslizPendiente(maxGolpePendienteIzq, ref cantidadMovimiento);
            MaxDeslizPendiente(maxGolpePendienteDer, ref cantidadMovimiento);
        }

        if (!colisiones.maxDeslicePendiente)//Ep13 si no esta deslizandoce entonces descienda a traves del input
        {
            float direccionX = Mathf.Sign (cantidadMovimiento.x);
		    Vector2 origenRayo = ((direccionX == -1 )? origenRayos.inferiorDerecha : origenRayos.inferiorIzquierda);
		    RaycastHit2D golpe = Physics2D.Raycast (origenRayo, -Vector2.up, Mathf.Infinity, mascaraColision);//rayo infinito
		    Debug.DrawRay (origenRayo,-Vector2.up * Mathf.Infinity, Color.black);//Mathf.Infinity

		    if (golpe) {
			    float anguloInclinacion = Vector2.Angle (golpe.normal, Vector2.up);
			    //Si es diferente de una superficie plana y si es menor o igual al angulo maximo de descenso
			    if (anguloInclinacion != 0 && anguloInclinacion <= maxAngulo) {
				    //Verifica si estamos en direccion o en contra la pendiente, lo que indica que estamos bajando por ella
				    if (Mathf.Sign (golpe.normal.x) == direccionX) {
                        //Como el rayo es infinito, verifica que estemos lo suficientemente cerca a la pendiente para tomar un efecto diferente al de caer despacio(gravedad cero) 
                        //al estar sobre una pendiente. 
                        //si nuestra distancia a la pendiente es menor o igual a que tan abajo tendremos que movernos en el eje Y basados en el angulo y en la cantidadMovimiento en el eje X,
                        //entonces estamos lo suficientemente cerca para entrar en efecto
                        if (golpe.distance - anchoPiel <= Mathf.Tan (anguloInclinacion * Mathf.Deg2Rad) * Mathf.Abs (cantidadMovimiento.x)) {//estamos buscando cateto opuesto/tambien puedo usar seno
						    float distanciaMovimiento = Mathf.Abs (cantidadMovimiento.x);//Hipotenusa
						    float cantidadMovimientoDescensoY= Mathf.Sin (anguloInclinacion * Mathf.Deg2Rad) * distanciaMovimiento;//Busco Opuesto para Y cuento con Hipotenusa.
						    cantidadMovimiento.x = Mathf.Cos (anguloInclinacion * Mathf.Deg2Rad) * distanciaMovimiento * Mathf.Sign(cantidadMovimiento.x);//Busco Adyacente para X porque cuento con Hipo.
						    cantidadMovimiento.y -= cantidadMovimientoDescensoY;
						    colisiones.anguloInclinacion = anguloInclinacion;
						    colisiones.bajandoPendiente = true;
						    colisiones.abajo = true;
                            //	print ("disMov: " + distanciaMovimiento + "Seno: "+ Mathf.Sin (anguloInclinacion* Mathf.Deg2Rad) * distanciaMovimiento);
                            colisiones.pendienteNormal = golpe.normal;
                        }
				    }
			    }
		    }
        }
	}
    //Ep13
    public void MaxDeslizPendiente(RaycastHit2D golpe,ref Vector2 cantidadMovimiento)
    {
        if (golpe)
        {
            float anguloInclinacion = Vector2.Angle(golpe.normal, Vector2.up);
            if(anguloInclinacion > maxAngulo)
            { 
                cantidadMovimiento.x = golpe.normal.x * (Mathf.Abs(cantidadMovimiento.y) - golpe.distance) / Mathf.Tan(anguloInclinacion * Mathf.Deg2Rad);
                colisiones.anguloInclinacion = anguloInclinacion;
                colisiones.maxDeslicePendiente = true;
                colisiones.pendienteNormal = golpe.normal;
            }
        }
    }
    #region movido
    /* Todo este fragmento es actualizado en el episodio 6 al script Raycast Controlador
	//Episodio1
	void ActualizarOrigenRayos(){
		Bounds limites = colisionador.bounds;
		limites.Expand (anchoPiel * -2);

		origenRayos.inferiorIzquierda = new Vector2 (limites.min.x, limites.min.y);
		origenRayos.inferiorDerecha = new Vector2 (limites.max.x, limites.min.y);
		origenRayos.superiorIzquierda = new Vector2 (limites.min.x, limites.max.y);
		origenRayos.superiorDerecha = new Vector2 (limites.max.x, limites.max.y);
	}

	//Episodio1
	void CalcularEspacioRayos(){
		Bounds limites = colisionador.bounds;
		limites.Expand (anchoPiel * -2);

		conteoRayosHorizontales = Mathf.Clamp (conteoRayosHorizontales, 2, int.MaxValue);
		conteoRayosVerticales = Mathf.Clamp (conteoRayosVerticales, 2, int.MaxValue);

		espacioRayoHorizontal = limites.size.y / (conteoRayosHorizontales - 1);
		espacioRayoVertical = limites.size.x / (conteoRayosVerticales - 1);
	}

	//Episodio1
	struct OrigenRayos{
		public Vector2 superiorIzquierda, superiorDerecha;
		public Vector2 inferiorIzquierda, inferiorDerecha;
	}*/
    #endregion
    //Ep10
    public void ResetearCaerAtravesPlataforma()
    {
        colisiones.caerAtravesPlataforma = false;
    }
	//Ep3 Identificar que colisiones estan ocurriendo
	public struct ColisionInfo{
		public bool arriba, abajo;//sirve para revisar y que no se acumule la gravedad en Jugador.cs, salto tambien
		public bool izquierda, derecha;	

		public bool subiendoPendiente;
		public bool bajandoPendiente;
        public bool maxDeslicePendiente;//ep13

		public float anguloInclinacion, anguloInclinaciónAntiguo;//el angulo antiguo significa el angulo del frame anterior
        public Vector2 pendienteNormal;
		public Vector2 cantidadMovimientoAntigua;
        public int caraDir;//Ep9
        public bool caerAtravesPlataforma;//Ep10

		public void Reset(){
			arriba = abajo = false;
			izquierda = derecha = false;
			subiendoPendiente = false;
			bajandoPendiente = false;
            maxDeslicePendiente = false;
            pendienteNormal = Vector2.zero;

            anguloInclinaciónAntiguo = anguloInclinacion;
			anguloInclinacion = 0;
		}
	}
}
/* problemas resueltos
 * Colisiones horizontales
 * Colisiones verticales
 * Salto en pendientes
 * cantidadMovimiento continua en ascenso por pendientes
 * Evitar que el collider descienda despegado de la pendiente
 * *Evitar que el collider vibre indeseadamente cuando se encuentra un obstaculo arriba de el en una pendiente
 *Evitar que el collider vibre indeseadamente cuando se encuentra un obstaculo al frente de el en una pendiente
 * cantidadMovimiento continua al encontrar una pendiente nueva mientras sube una
 * cantidadMovimiento continua al descender pendientes
 */