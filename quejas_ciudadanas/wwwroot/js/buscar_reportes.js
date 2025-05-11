

$('#problemaSelect').on('change', function () {
    var idProblema = $(this).val();
    $('#claseProblemaSelect').prop('disabled', true).empty().append('<option value="">Cargando...</option>');
    $('#tipoProblemaSelect').prop('disabled', true).empty().append('<option value="">Cargando..</option>');

    if (idProblema) {
        $.getJSON('/Reportes/GetClasesProblema/' + idProblema, function (data) {
            $('#claseProblemaSelect').empty().append('<option value="" disabled selected>Seleccione una clase de problema</option>');
            $.each(data, function (i, item) {
                $('#claseProblemaSelect').append($('<option>', {
                    value: item.id_Clase_Problema,
                    text: item.nombre
                }));
            });
            $('#claseProblemaSelect').prop('disabled', false);
        });
    }
});

$('#claseProblemaSelect').on('change', function () {
    var idClase = $(this).val();
    $('#tipoProblemaSelect').prop('disabled', true).empty().append('<option value="">Cargando...</option>');

    if (idClase) {
        $.getJSON('/Reportes/GetTiposProblema/' + idClase, function (data) {
            $('#tipoProblemaSelect').empty().append('<option value="" disabled selected>Seleccione un tipo de problema</option>');
            $.each(data, function (i, item) {
                $('#tipoProblemaSelect').append($('<option>', {
                    value: item.id_Tipo_Problema,
                    text: item.nombre
                }));
            });
            $('#tipoProblemaSelect').prop('disabled', false);
        });
    }
});

$('#selectDepto').change(function () {
    var codDepto = $(this).val();
    if (codDepto) {
        $.get('/Reportes/ObtenerCiudades', { codDepto: codDepto }, function (data) {
            var ciudadSelect = $('#selectCiudad');
            ciudadSelect.empty().append('<option value="00" disabled selected >Seleccione Ciudad</option>');
            $.each(data, function (i, ciudad) {
                ciudadSelect.append('<option value="' + ciudad.a002CODUBICACION + '">' + ciudad.a002DESCRIPCION + '</option>');
            });
            ciudadSelect.prop('disabled', false);
        });
    } else {
        $('#selectCiudad').empty().append('<option value="" disabled>Seleccione Ciudad</option>').prop('disabled', true);
    }
});



//-------------------manejo de mapa y tabla ---------------------------------

const estadoTextos = { 1: "Pendiente", 2: "En proceso", 3: "Finalizado" };
const estadoColores = { 1: "#003366", 2: "#FFD700", 3: "#FF0000" };
let data = []; // Aquí se almacenarán los datos de las quejas

const map = L.map('map').setView([4.65, -74.1], 12);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);
const marcadores = {};

var id_marcador = "";
// Llamada AJAX para obtener los datos
function cargarDatos() {
    mostrarPreloader();
    ocultar_detalles();
    limpiarMarcadores();
    limpiarTabla();
    //-------------------------
    // Captura de datos
    function limpiarValor(valor) {
        return valor === "null" || valor === null || valor === "" ? "000000" : valor;
    }

    const problemaSelect = $("#problemaSelect").val();
    const claseProblemaSelect = $("#claseProblemaSelect").val();
    const tipoProblemaSelect = $("#tipoProblemaSelect").val();
    const selectDepto = limpiarValor($("#selectDepto").val());
    const selectCiudad = limpiarValor($("#selectCiudad").val());
    const Estado_del_caso = $("#Estado_del_caso").val();
    // Si todo está correcto, construir FormData (si enviarás con Ajax)
    var formData = new FormData();
    formData.append("problemaSelect", problemaSelect);
    formData.append("claseProblemaSelect", claseProblemaSelect);
    formData.append("tipoProblemaSelect", tipoProblemaSelect);
    formData.append("selectDepto", selectDepto);
    formData.append("selectCiudad", selectCiudad);
    formData.append("Estado_del_caso", Estado_del_caso);
    //-------------------------
    $.ajax({
        url: '/Reportes/consultar_reportes', // URL de la acción en el controlador
        method: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            if (response.length>0) { id_marcador = response[0].id; }
            else { id_marcador = ""; }
            data = response;
            data.forEach(crearMarcador);
            renderTabla();
            ocultarPreloader();
        }
    });
}

$(".enviar_formulario_boton").on("click", function (e) {
    e.preventDefault();
    cargarDatos();
});

function crearMarcador(item) {
    const icon = L.divIcon({
        html: `<div style="background:${estadoColores[item.estado]};color:white;padding:4px 6px;border-radius:15px;border:2px solid #fff;font-size:12px;box-shadow:0 0 3px rgba(0,0,0,0.5);transform:translate(-50%,-50%)">${item.cantidad}</div>`,
        className: '',
        iconSize: [30, 30],
        iconAnchor: [15, 15]
    });
    const marker = L.marker([item.lat, item.lon], { icon }).addTo(map)
        .bindTooltip(item.titulo).on('click', () => mostrarAlerta(item.id,0));
    marcadores[item.id] = marker;

    if (id_marcador != "") { centrarEnMapa(id_marcador); }
}



function centrarEnMapa(id) {
    const marker = marcadores[id];
    if (marker) {
        map.setView(marker.getLatLng(), 15);
        marker.openTooltip();
    }
}

// Paginación y renderizado
let paginaActual = 1;
const porPagina = 10;
let ordenActual = { campo: null, asc: true };

function renderTabla() {
    const inicio = (paginaActual - 1) * porPagina;
    const paginados = [...dataOrdenada()].slice(inicio, inicio + porPagina);

    const cuerpo = document.getElementById('tablaBody');
    cuerpo.innerHTML = '';
    for (const item of paginados) {
        cuerpo.innerHTML += `
          <tr>
            <td>${item.problema}</td>
            <td>${item.clase}</td>
            <td>${item.tipo}</td>
            <td>${item.departamento}</td>
            <td>${item.ciudad}</td>
            <td>${item.fecha}</td>
            <td>${estadoTextos[item.estado]}</td>
            <td>${item.cantidad}</td>
            <td><button class="btn btn-sm btn-primary" onclick="centrarEnMapa(${item.id})"><i class="bi bi-geo-alt-fill"></i></button></td>
            <td><button class="btn btn-sm btn-warning" onclick="mostrarAlerta(${item.id},0)"><i class="bi bi-caret-right-fill"></i></button></td>

          </tr>`;
    }

    renderPaginacion();
}

function renderPaginacion() {
    const totalPaginas = Math.ceil(data.length / porPagina);
    const paginador = document.getElementById('paginacion');
    paginador.innerHTML = '';

    for (let i = 1; i <= totalPaginas; i++) {
        paginador.innerHTML += `
          <li class="page-item ${i === paginaActual ? 'active' : ''}">
            <a class="page-link" href="#" onclick="irPagina(${i})">${i}</a>
          </li>`;
    }
}

function irPagina(num) {
    paginaActual = num;
    renderTabla();
}

function ordenarTabla(campo) {
    if (ordenActual.campo === campo) {
        ordenActual.asc = !ordenActual.asc;
    } else {
        ordenActual = { campo, asc: true };
    }
    renderTabla();
}

function dataOrdenada() {
    const { campo, asc } = ordenActual;
    if (!campo) return data;
    return [...data].sort((a, b) => {
        if (typeof a[campo] === 'number') {
            return asc ? a[campo] - b[campo] : b[campo] - a[campo];
        } else {
            return asc ? a[campo].localeCompare(b[campo]) : b[campo].localeCompare(a[campo]);
        }
    });
}

// Inicialización
cargarDatos();

// Agregar leyenda al mapa
const legend = L.control({ position: 'bottomright' });
legend.onAdd = function () {
    const div = L.DomUtil.create('div', 'legend');
    div.innerHTML = '<b>Estados</b><br>';
    for (let key in estadoTextos) {
        div.innerHTML += `
          <i style="background:${estadoColores[key]}"></i>
          ${estadoTextos[key]}<br>`;
    }
    return div;
};
legend.addTo(map);

//


// Función para limpiar tabla y mapa
function limpiarMarcadores() {
    for (let id in marcadores) {
        map.removeLayer(marcadores[id]);
    }
    // Vaciar el objeto de marcadores
    for (let id in marcadores) {
        delete marcadores[id];
    }
}
function limpiarTabla() {
    document.getElementById('tablaBody').innerHTML = '';
    document.getElementById('paginacion').innerHTML = '';
}



//--------------manejo del slider-------------------------

function previousSlide() {
    const carouselElement = document.getElementById('carouselExampleIndicators');
    const carousel = bootstrap.Carousel.getInstance(carouselElement);
    carousel.prev();
}

function nextSlide() {
    const carouselElement = document.getElementById('carouselExampleIndicators');
    const carousel = bootstrap.Carousel.getInstance(carouselElement);
    carousel.next();
}


// imagenes
// Array con solo las URLs de las imágenes

// Función para generar el carrusel
function generarCarrusel(imagenes) {
    const carouselInner = document.querySelector('.carousel-inner');

    // Limpiar el contenido existente
    carouselInner.innerHTML = '';

    // Generar los elementos del carrusel
    imagenes.forEach((imagen, index) => {imagen.id_reporte
        const carouselItem = document.createElement('div');
        carouselItem.className = `carousel-item ${index === 0 ? 'active' : ''}`;

        carouselItem.innerHTML = `
                    <img src="${imagen.id_reporte}" class="d-block w-100" alt="Imagen ${index + 1}">
                `;

        carouselInner.appendChild(carouselItem);
    });
}




//-----------manejo de reportes asociados al caso----------------------------------------------
// Datos de los grupos y proyectos

// Función para generar un grupo con sus tarjetas
function generarGrupo(grupo) {
    let tarjetasHTML = '';

    // Generar cada tarjeta del grupo
    grupo.proyectos.forEach((proyecto, index) => {
        tarjetasHTML += `
                    <div class="card ${index === 0 ? '' : 'mt-3'}">
                        <div class="card-header informes_titulo_reportes">
                            <h5 class="card-title mb-0">${proyecto.nombre_Completo} - ${proyecto.num_identificacion} </h5>
                            <span class="date-badge informes_titulo_reportes_spam">Registro: ${proyecto.fecha_registro}</span>
                        </div>
                        <div class="card-body">
                            <p class="card-text justifi_text">${proyecto.descripcion}</p>
                            <div class="text-end informes_titulo_reportes_lupa">
                                <button class="btn btn-primary btn-lupa " title="Ver detalles" data-nombre="${proyecto.id_reporte}"  onclick="mostrarAlerta_relacionadas(${proyecto.id_caso},${proyecto.id_reporte})">
                                    <i class="bi bi-search"></i>
                                </button>
                            </div>
                        </div>
                    </div>
                `;
    });

    // Crear el grupo completo con contenedor scrollable
    return `
                <div class="list-group-item">
                    <div class="list-group-header mb-2 informes_titulo">${grupo.nombre}</div>
                    <div class="proyectos-container">
                        ${tarjetasHTML}
                    </div>
                </div>
            `;
}

// Función para generar toda la lista agrupada
function generarListaAgrupada(grupos) {
    const contenedorLista = document.getElementById('listaAgrupada');
    let listaHTML = '';
    listaHTML += generarGrupo(grupos);
    contenedorLista.innerHTML = listaHTML;
    // Generar cada grupo grupos.id
    //grupos.forEach(grupo => {
    //    listaHTML += generarGrupo(grupo);
    //});

    // Insertar en el DOM
   

    // Añadir eventos a los botones de lupa
    //const botonesLupa = document.querySelectorAll('.btn-lupator');
    //botonesLupa.forEach(boton => {
    //    boton.addEventListener('click', function () {
    //        const nombreProyecto = this.getAttribute('data-nombre');
    //        alert(`Has seleccionado: ${nombreProyecto}`);
    //    });
    //});
}


//---------manejo de respuestas del funcionario-----------------------------------------------

function generarListaDocumentos(documentos, containerId = 'documentosContainer', options = {}) {
    // Opciones por defecto
    const config = {
        titulo: options.titulo || 'Documentos',
        mensajeSinDocumentos: options.mensajeSinDocumentos || 'No hay documentos disponibles',
        mensajeDescripcion: options.mensajeDescripcion || 'No se encontraron documentos que mostrar en este momento.',
        textoBoton: options.textoBoton || 'Descargar PDF',
        claseBoton: options.claseBoton || 'boton-descargar'
    };

    // Seleccionar el contenedor
    const contenedor = document.getElementById(containerId);
    if (!contenedor) {
        console.error(`El contenedor con ID '${containerId}' no existe en el documento.`);
        return;
    }

    // Limpiar el contenedor antes de agregar elementos
    contenedor.innerHTML = '';

    // Verificar si hay documentos para mostrar
    if (!documentos || documentos.length === 0) {
        // Crear y mostrar el mensaje de no hay documentos
        const mensajeSinDocs = document.createElement('div');
        mensajeSinDocs.className = 'mensaje-sin-documentos';
        mensajeSinDocs.innerHTML = `
                    <i class="bi bi-folder-x icono-sin-documentos"></i>
                    <h3>${config.mensajeSinDocumentos}</h3>
                    <p class="text-muted">${config.mensajeDescripcion}</p>
                `;
        contenedor.appendChild(mensajeSinDocs);
        return;
    }

    // Si hay documentos, crear la lista
    const listaDocumentos = document.createElement('div');

    // Iterar sobre los datos para crear cada elemento
    documentos.forEach(doc => {
        // Crear el elemento principal
        const elementoLista = document.createElement('div');
        elementoLista.className = 'elemento-lista';
        elementoLista.id = `documento-${doc.id || Math.random().toString(36).substr(2, 9)}`;

        // Crear cabecera
        const cabecera = document.createElement('div');
        cabecera.className = 'cabecera-elemento informes_titulo';

        const titulo = document.createElement('h3');
        titulo.className = 'titulo-elemento informes_titulo_reportes_spam';
        titulo.textContent = doc.titulo || 'Sin título';

        const fecha = document.createElement('span');
        fecha.className = 'fecha-elemento';
        fecha.textContent = doc.fecha || 'Sin fecha';

        cabecera.appendChild(titulo);
        cabecera.appendChild(fecha);

        // Crear contenido
        const contenido = document.createElement('div');
        contenido.className = 'contenido-elemento';

        // Agregar descripción si existe
        if (doc.descripcion) {
            const descripcion = document.createElement('p');
            descripcion.className = 'descripcion-elemento justifi_text';
            descripcion.textContent = doc.descripcion;
            contenido.appendChild(descripcion);
        }

        // Verificar si hay imágenes para mostrar
        if (doc.imagenes && doc.imagenes.length > 0) {
            const galeriaImagenes = document.createElement('div');
            galeriaImagenes.className = 'galeria-imagenes';

            // Ajustar el grid según el número de imágenes
            if (doc.imagenes.length === 1) {
                galeriaImagenes.style.gridTemplateColumns = '1fr';
            } else if (doc.imagenes.length === 2) {
                galeriaImagenes.style.gridTemplateColumns = 'repeat(2, 1fr)';
            }

            // Agregar imágenes
            doc.imagenes.forEach(img => {
                const contenedorImagen = document.createElement('div');
                contenedorImagen.className = 'contenedor-imagen';

                const imagen = document.createElement('img');
                imagen.src = img.url || '';
                imagen.alt = img.alt || 'Imagen';
                imagen.className = 'imagen-contenido';

                contenedorImagen.appendChild(imagen);
                galeriaImagenes.appendChild(contenedorImagen);
            });

            // Agregar galería al contenido
            contenido.appendChild(galeriaImagenes);
        }

        // Verificar si hay una URL de PDF válida para mostrar el botón
        if (doc.pdfUrl && doc.pdfUrl.trim() !== '') {
            // Crear botón de descarga
            const botonDescargar = document.createElement('a');
            botonDescargar.href = doc.pdfUrl;
            botonDescargar.className = config.claseBoton;
            botonDescargar.target = '_blank';
            botonDescargar.innerHTML = `
                        <i class="bi bi-file-earmark-pdf icono-descargar"></i>
                        ${config.textoBoton}
                    `;

            // Agregar botón al contenido
            contenido.appendChild(botonDescargar);
        }

        // Construir el elemento completo
        elementoLista.appendChild(cabecera);
        elementoLista.appendChild(contenido);

        // Agregar el elemento a la lista
        listaDocumentos.appendChild(elementoLista);
    });

    // Agregar la lista al contenedor
    contenedor.appendChild(listaDocumentos);
}


// Ejemplos de diferentes escenarios
function simularListaVacia() {
    generarListaDocumentos([]);
}

function simularLista() {
    generarListaDocumentos(documentosEjemplo);
}

// Función para cargar documentos desde una API
function cargarDocumentosDesdeAPI() {
    fetch('https://api.ejemplo.com/documentos')
        .then(response => response.json())
        .then(data => {
            generarListaDocumentos(data);
        })
        .catch(error => {
            console.error('Error al cargar documentos:', error);
            generarListaDocumentos([]); // Mostrar mensaje de no hay documentos en caso de error
        });
}


//---------manejo de cara del caso-----------------------------------------------
// Función para actualizar la carta con cualquier objeto de datos
function actualizarCarta(datos) {
    // Actualizar campos básicos
    document.getElementById('titulo').textContent = datos.titulo || 'Sin título';
    document.getElementById('fecha').textContent = datos.fecha || '';
    document.getElementById('problema').textContent = datos.problema || '-';
    document.getElementById('clase').textContent = datos.clase || '-';
    document.getElementById('tipo').textContent = datos.tipo || '-';
    document.getElementById('departamento').textContent = datos.departamento || '-';
    document.getElementById('ciudad').textContent = datos.ciudad || '-';
    document.getElementById('direccion').textContent = datos.direccion || '-';
    document.getElementById('reportes').textContent = datos.cantidadReportes || '0';
    document.getElementById('descripcion').textContent = datos.descripcion || '-';

    // Actualizar los nuevos campos
    document.getElementById('tipoDocumento').textContent = datos.tipoDocumento || '-';
    document.getElementById('numeroIdentificacion').textContent = datos.numeroIdentificacion || '-';
    document.getElementById('nombreReportador').textContent = datos.nombreReportador || '-';

    // Mapeos para estados numéricos a texto y colores
    const estadoTextos = { 1: "Pendiente", 2: "En proceso", 3: "Finalizado" };
    const estadoColores = { 1: "#003366", 2: "#FFD700", 3: "#FF0000" };

    // Actualizar el estado y su color según el valor numérico
    const estadoElement = document.getElementById('estado');
    const estadoNumerico = parseInt(datos.estado);

    if (estadoNumerico && estadoTextos[estadoNumerico]) {
        // Si es un número válido en nuestro mapeo
        estadoElement.textContent = estadoTextos[estadoNumerico];
        estadoElement.className = 'badge estado-badge'; // Resetear clases y mantener la clase estado-badge
        estadoElement.style.backgroundColor = estadoColores[estadoNumerico];
        // Asegurar legibilidad del texto basado en el color de fondo
        if (estadoNumerico === 2) { // Para amarillo (color claro)
            estadoElement.style.color = '#000';
        } else {
            estadoElement.style.color = '#fff';
        }
    } else {
        // Valor por defecto o no reconocido
        estadoElement.textContent = datos.estado || '-';
        estadoElement.className = 'badge estado-badge bg-secondary';
        estadoElement.style.backgroundColor = '';
    }

    // Actualizar evento del botón si se proporciona una función
    if (datos.onResponder && typeof datos.onResponder === 'function') {
        document.getElementById('responder').onclick = datos.onResponder;
    }
}

// Datos de ejemplo para mostrar


// Ejemplo de cómo usar la función para actualizar con datos diferentes
document.getElementById('responder').addEventListener('click', function () {
    // Puede hacer lo que necesite aquí
    console.log("Botón responder presionado");
});

// Inicializar la carta con datos de ejemplo al cargar



// Generar la lista agrupada al cargar la página
//document.addEventListener('DOMContentLoaded', () => {
//    //generarCarrusel(imagenes);
//    /*generarListaAgrupada(grupos);*/
//    generarListaDocumentos(documentosEjemplo);
//    //actualizarCarta(datosEjemplo);
//});

///--------------generar consulta -----------------------
function mostrarreportes_relacionados(id_caso) {
    //const item = data.find(d => d.id === id);
    ///
    var formData = new FormData();
    formData.append("IdCaso", id_caso);
    formData.append("IdReporte", null);
    ///
    $.ajax({
        url: '/Reportes/lista_reportes_similares', // URL de la acción en el controlador
        method: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            casos_asociados = response;
            generarListaAgrupada(casos_asociados);

        }
    });
    ///
    $('.table_ubicacion_inicio').show();

}

function mostrarAlerta(id_caso, id_report) {
    //const item = data.find(d => d.id === id);
    $('.class_reportes_respuesta').hide();
    mostrar_detalles();
    ///
    var formData = new FormData();
    formData.append("IdCaso", id_caso);
    formData.append("IdReporte", id_report);
    ///
    $.ajax({
        url: '/Reportes/consultar_caso_x_report', // URL de la acción en el controlador
        method: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            //console.log(response)
            //console.log(response.info)
            //console.log(response.fotos)
            actualizarCarta(response.info[0]);
            generarCarrusel(response.fotos);
            mostrarreportes_relacionados(response.info[0].id_caso)
            $("html, body").animate({ scrollTop: 600 }, 100);
            //
            $('#caso_responder_form').val(response.info[0].id_caso);
            mostrarReportesRelacionados(response.info[0].id_caso);
            //id_marcador = response[0].id;
        }
    });
    ///
    $('.table_ubicacion_inicio').show();

}

function mostrarAlerta_relacionadas(id_caso, id_report) {
    //const item = data.find(d => d.id === id);
    ///
    var formData = new FormData();
    formData.append("IdCaso", id_caso);
    formData.append("IdReporte", id_report);
    ///
    $.ajax({
        url: '/Reportes/consultar_caso_x_report', // URL de la acción en el controlador
        method: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            //console.log(response)
            //console.log(response.info)
            //console.log(response.fotos)
            actualizarCarta(response.info[0]);
            generarCarrusel(response.fotos);
            $("html, body").animate({ scrollTop: 600 }, 100);
            //
            $('#caso_responder_form').val(response.info[0].id_caso);
            mostrarReportesRelacionados(response.info[0].id_caso);
            //id_marcador = response[0].id;
        }
    });
    ///
    $('.table_ubicacion_inicio').show();

}
////-------------llamar respuestas del agente ---------------------------------------
function mostrarReportesRelacionados(IdRespAgente) {
    $.ajax({
        url: '/Reportes/consultar_respuesta_agentes',
        type: 'POST',
        data: {
            IdCaso: IdRespAgente // Se pasa el IdRespAgente como parámetro
        },
        success: function (data) {
            if (data.length === 0) {
                console.log("No se encontraron resultados.");
            } else {
                console.log("Resultados recibidos:", data);

                //// Ejemplo de uso
                //data.forEach(doc => {
                //    console.log("Título:", doc.titulo);
                //    console.log("Fecha:", doc.fecha);
                //    console.log("Descripción:", doc.descripcion);
                //    console.log("PDF:", doc.pdfUrl);
                //    console.log("Imágenes:", doc.imagenes);
                //});
                $('.class_reportes_respuesta').show();
                generarListaDocumentos(data);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error al consultar:", error);
        }
    });
}

///----------------------------------------------------------------------------------
document.getElementById('formularioEnvio').addEventListener('submit', function (e) {
    e.preventDefault();

    const form = document.getElementById('formularioEnvio');
    const formData = new FormData(form);

    // Validación adicional antes de enviar
    const fotos = document.getElementById('fotos').files;
    if (fotos.length > 3) {
        alert("Solo puedes subir hasta 3 imágenes.");
        return;
    }

    // Validar que todas las imágenes sean de tipo válido (por ejemplo, jpg, jpeg, png)
    for (let i = 0; i < fotos.length; i++) {
        const extension = fotos[i].type.split('/')[1].toLowerCase();
        if (!['jpeg', 'jpg', 'png'].includes(extension)) {
            alert("Solo se permiten imágenes con los formatos .jpg, .jpeg, .png.");
            return;
        }
    }

    const archivoPdf = document.getElementById('archivoPdf').files[0];
    if (archivoPdf && archivoPdf.type !== "application/pdf") {
        alert("Solo se permite un archivo PDF.");
        return;
    }

    fetch('/Reportes/Enviar_Respuest_caso', {
        method: 'POST',
        body: formData
    })
        .then(resp => {
            if (!resp.ok) {
                throw new Error("Error en el servidor");
            }
            return resp.json();
        })
        .then(data => {
            // Mostrar mensaje de éxito
            const mensaje = document.getElementById('mensajeFinal');
            mensaje.classList.remove('d-none');
            mensaje.textContent = data.mensaje;

            // Limpiar formulario
            form.reset();

            // Cerrar modal si existe
            const modal = bootstrap.Modal.getInstance(document.getElementById('formModal'));
            if (modal) modal.hide();

            // Recargar la página después de 2 segundos (opcional)
            setTimeout(function () {
                location.reload(); // Recarga la página
            }, 2000); // Espera 2 segundos antes de recargar
        })
        .catch(err => {
            alert("caso actualizado.");
            console.error(err);
            setTimeout(function () {
                location.reload(); // Recarga la página
            }, 2000);
        });
});

////------------------------------------------------------


function ocultar_detalles(){
    $('.table_ubicacion_inicio').hide()
}
function mostrar_detalles() {
    $('.table_ubicacion_inicio').show()
}

function mostrarPreloader() {
    // Mostrar el preloader
    document.getElementById("preloader").style.display = "flex";
}

function ocultarPreloader() {
    // Ocultar el preloader
    document.getElementById("preloader").style.display = "none";
}

///-------------------RESPONDER CASO--------------------------------------------

////-----------------------------------------------------------------------------












