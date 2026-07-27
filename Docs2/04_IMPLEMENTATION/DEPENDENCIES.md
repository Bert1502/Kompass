# DEPENDENCIES

Erlaubt: API/Desktop → Application → Domain; Persistence implementiert Application-Abstraktionen. Verboten: Domain → EF/HTTP/WPF, Controller/ViewModel → DbContext für Fachabläufe, Persistence → Desktop.
