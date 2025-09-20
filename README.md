## Proyecto web Real State

- El proyecto esta hecho en .NET 8 C# como backend para la API y Next.js como frontend para mostrar la interfaz

1. Backend API .NET C#
   En la carpeta api esta el backend en .NET y C# donde se exponen los endpoints para listar todas las propiedades, buscar por nombre, direccion, precio minimo y precio maximo,
   los cuales se encuentran almacenados en una base de datos mongodb.

   Para correr el proyecto se debe ingresar desde la consola a la carpeta:
  
   cd WebAPIRealState/api
   dotnet watch run
   Enter

   Quedara expuesta la api en la siguiente url: http://localhost:5173/api/properties

2. Frontend Next.js
   En la carpeta frontend esta el consumo de la api con Next.js el cual muestra un listado de propiedades y permite filtrar por nombre, direccion, precio maximo y precio minimo.

   Para correr el proyecto se debe ingresar desde la consola a la carpeta:

   cd WebAPIRealState/frontend
   npm run dev
   Enter

   La aplicacion puede ser accedida desde el navegador en la siguiente url: http://localhost:3000.

3. Base de Datos
   En la carpeta bd esta el esquema de la bd mongodb en el archivo: RealStateBd.Properties.json que contiene el json exportado MongoDB Compass.
   
   DatabaseName: RealStateBd
   CollectionName: Properties

4. Conclusiones
   La desicion del esquema de la base de datos es por que me va permitir realizar consultas mas rapidas sobre las propiedades y los datos de las mismas no van a cambiar con mucha frecuencia.

   Los patrones usados en Next.js son Presentational & Container para separar el contenedor de cada tarjeta con información de cada propiedad,
   tambien use custom hooks para sacar la funcionalidad reutilizable fetchData que trae la informacion de la api
