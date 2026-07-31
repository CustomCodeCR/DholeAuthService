namespace Dhole.Auth.Persistence.Seed;

internal static class StorageScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
        [
            ScopeCatalog.Create(
                "storage.files.create",
                "Cargar archivos en Storage",
                "Permite cargar archivos y asociarlos con entidades de los servicios."
            ),
            ScopeCatalog.Create(
                "storage.files.view",
                "Ver archivos de Storage",
                "Permite consultar el panel, metadatos, referencias y versiones de archivos."
            ),
            ScopeCatalog.Create(
                "storage.files.download",
                "Descargar archivos de Storage",
                "Permite descargar el contenido almacenado."
            ),
            ScopeCatalog.Create(
                "storage.files.delete",
                "Eliminar archivos de Storage",
                "Permite eliminar archivos y sus versiones físicas."
            ),
            ScopeCatalog.Create(
                "storage.files.version",
                "Administrar versiones de Storage",
                "Permite cargar nuevas versiones y cambiar la versión actual de un archivo."
            ),
            ScopeCatalog.Create(
                "storage.providers.view",
                "Ver proveedores de Storage",
                "Permite consultar los proveedores de almacenamiento configurados."
            ),
            ScopeCatalog.Create(
                "storage.providers.create",
                "Crear proveedores de Storage",
                "Permite registrar proveedores Local, MinIO, S3 o Azure Blob."
            ),
            ScopeCatalog.Create(
                "storage.providers.update",
                "Actualizar proveedores de Storage",
                "Permite modificar proveedores y su configuración."
            ),
            ScopeCatalog.Create(
                "storage.providers.set-active",
                "Activar/Inactivar proveedores de Storage",
                "Permite activar o inactivar proveedores de almacenamiento."
            ),
        ];
}
