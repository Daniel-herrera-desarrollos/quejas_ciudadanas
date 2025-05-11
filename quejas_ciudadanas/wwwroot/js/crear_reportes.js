const BASE32 = "0123456789bcdefghjkmnpqrstuvwxyz";
function encodeGeohash(latitude, longitude, precision = 9) {
    let isEven = true, bit = 0, ch = 0, geohash = "";
    let lat = [-90.0, 90.0], lon = [-180.0, 180.0];
    while (geohash.length < precision) {
        const mid = isEven ? (lon[0] + lon[1]) / 2 : (lat[0] + lat[1]) / 2;
        if ((isEven ? longitude : latitude) > mid) {
            ch |= 1 << (4 - bit);
            isEven ? lon[0] = mid : lat[0] = mid;
        } else {
            isEven ? lon[1] = mid : lat[1] = mid;
        }
        isEven = !isEven;
        if (++bit === 5) {
            geohash += BASE32[ch]; bit = 0; ch = 0;
        }
    }
    return geohash;
}

const map = L.map('map').setView([4.60971, -74.08175], 13);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '© OpenStreetMap contributors'
}).addTo(map);

let marker = null;
function setMarker(lat, lon) {
    if (marker) map.removeLayer(marker);
    marker = L.marker([lat, lon]).addTo(map);
    updateData(lat, lon);
}

function updateData(lat, lon) {
    document.getElementById("lat").textContent = lat.toFixed(6);
    document.getElementById("lon").textContent = lon.toFixed(6);
    document.getElementById("geohash").textContent = encodeGeohash(lat, lon);
    fetch(`https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${lat}&lon=${lon}`)
        .then(res => res.json())
        .then(data => {
            const addr = data.address || {};
            const direccion = data.display_name || "-";
            document.getElementById("direccionInput").value = direccion;
        });
}

map.on('click', e => setMarker(e.latlng.lat, e.latlng.lng));

function ubicarme() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(pos => {
            const lat = pos.coords.latitude, lon = pos.coords.longitude;
            map.setView([lat, lon], 15);
            setMarker(lat, lon);
        });
    } else alert("Tu navegador no soporta geolocalización");
}

function previewImage(event, index) {
    const file = event.target.files[0];
    if (file && file.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onload = e => {
            document.getElementById(`preview${index}`).src = e.target.result;
            document.getElementById(`preview${index}`).style.display = "block";
            document.getElementById(`icon${index}`).style.display = "none";
        };
        reader.readAsDataURL(file);
    }
}




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
            ciudadSelect.empty().append('<option value="" disabled selected >Seleccione Ciudad</option>');
            $.each(data, function (i, ciudad) {
                ciudadSelect.append('<option value="' + ciudad.a002CODUBICACION + '">' + ciudad.a002DESCRIPCION + '</option>');
            });
            ciudadSelect.prop('disabled', false);
        });
    } else {
        $('#selectCiudad').empty().append('<option value="" disabled>Seleccione Ciudad</option>').prop('disabled', true);
    }
});


$(".enviar_formulario_boton").on("click", function (e) {
    e.preventDefault();
    mostrarPreloader();
    // Captura de datos
    const lat = $("#lat").text().trim();
    const lon = $("#lon").text().trim();
    const geohash = $("#geohash").text().trim();
    const tipoProblema = $("#tipoProblemaSelect").val();
    const ciudad = $("#selectCiudad").val();
    const direccion = $("#direccionInput").val().trim();
    const descripcion = $("#descripcion").val().trim();

    // Validación de campos vacíos
    if (lat === "-" || lon === "-" || geohash === "-" || !tipoProblema || !ciudad || direccion === "" || descripcion === "") {
        mostrarModal("Todos los campos son obligatorios.");
        return;
    }

    // Validación de las 6 imágenes
    for (let i = 0; i < 6; i++) {
        const fileInput = $("#file" + i)[0];
        if (!fileInput || fileInput.files.length === 0) {
            mostrarModal(`Debe cargar las 6 imagenes. La imagen ${i + 1} no ha sido seleccionada.`);
            return;
        }
    }

    // Si todo está correcto, construir FormData (si enviarás con Ajax)
    var formData = new FormData();
    formData.append("Latitud", lat);
    formData.append("Longitud", lon);
    formData.append("Geohash", geohash);
    formData.append("TipoProblema", tipoProblema);
    formData.append("Ciudad", ciudad);
    formData.append("Direccion", direccion);
    formData.append("Descripcion", descripcion);

    for (let i = 0; i < 6; i++) {
        const file = $("#file" + i)[0].files[0];
        formData.append("Imagen" + i, file);
    }

    // Envío AJAX
    $.ajax({
        url: '/Reportes/GuardarReporte', // Reemplaza por tu ruta real
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (res) {
            console.log(res);
            mostrarModal("Reporte enviado exitosamente.");
            ocultarPreloader();
        },
        error: function (err) {
            console.log(res);
            mostrarModal("Error al enviar el reporte.");
            ocultarPreloader();
        }
    });
});

// Función para mostrar modal
function mostrarModal(mensaje) {
    ocultarPreloader();
    $("#mensajeError").text(mensaje);
    const modal = new bootstrap.Modal(document.getElementById('modalError'));
    modal.show();
}







