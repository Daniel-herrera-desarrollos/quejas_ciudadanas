// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function desencriptarBase64_guardado() {
    // Texto en Base64
    const textoBase64 = "LyoKQ29weXJpZ2h0IChjKSAyMDI1IFtkYW5pZWwgYWxlamFuZHJvIGhlcnJlcmEgbWVqaWFdClRvZG9zIGxvcyBkZXJlY2hvcyByZXNlcnZhZG9zLgoKRXN0ZSBjw7NkaWdvIGZ1ZSBjcmVhZG8gcG9yIFtkYW5pZWwgYWxlamFuZHJvIGhlcnJlcmEgbWVqaWFdLCB5IG5vIHNlIHBlcm1pdGUgbGEgZGlzdHJpYnVjacOzbiBvIHVzbyBzaW4gZWwgY29uc2VudGltaWVudG8gZXhwcmVzbyBkZWwgYXV0b3IuCkZlY2hhIGRlIGNyZWFjacOzbjogTWF5byAyMDI1Ciov";

    // Desencriptar Base64 a texto
    const textoDesencriptado = atob(textoBase64);

    // Mostrar el texto desencriptado en un alert
    alert(textoDesencriptado);
}

function mostrarPreloader() {
    // Mostrar el preloader
    document.getElementById("preloader").style.display = "flex";
}

function ocultarPreloader() {
    // Ocultar el preloader
    document.getElementById("preloader").style.display = "none";
}


/*
Copyright (c) 2025 [daniel alejandro herrera mejia]
Todos los derechos reservados.

Este código fue creado por [daniel alejandro herrera mejia], y no se permite la distribución o uso sin el consentimiento expreso del autor.
Fecha de creación: Mayo 2025
*/



