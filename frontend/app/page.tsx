import PropertyCard from "./components/PropertyCard";
import ButtonMostrarTodos from "./components/ButtonMostrarTodos";

async function fetchData(filters: Record<string, string | number>) {

  const query = new URLSearchParams(
    Object.entries(filters)
    .filter(([_, v]) => v != null)
    .map(([k, v]) => [k, String(v)]) || {} // 👈 forzar a string
  );

  let urlAPI = "http://localhost:5173/api/properties";
  const urlAPIFilter = "/filter";

  const params = new URLSearchParams(query.toString());
  const name = params.get("name");
  const address = params.get("address");
  const minPrice = params.get("minPrice");
  const maxPrice = params.get("maxPrice");

  if(query.size > 0 && (name != "" || address != "" || minPrice != "" || maxPrice != "")) {
    urlAPI += urlAPIFilter;
  }

  const apiFull = urlAPI + "?" + query.toString();

  const res = await fetch(apiFull.toString(), {
    cache: "no-store", // evita cacheo en SSR
  });

  if (!res.ok) {
    throw new Error("No se pudieron obtener los datos");
  }

  return res.json();
}

export default async function Home({ searchParams }: { searchParams: Record<string, string> }) {
  const filters = {
    name: searchParams?.name || "",
    address: searchParams?.address || "",
    minPrice: searchParams?.minPrice || "",
    maxPrice: searchParams?.maxPrice || "",
  };

  const properties = await fetchData(filters);

  return (
    <div className="p-8 max-w-6xl mx-auto">
      <h1 className="text-2xl font-bold mb-4">🏡 Propiedades</h1>

      {/* Formulario que actualiza la URL */}
      <form className="flex gap-4 mb-6">
        <input
          type="text"
          name="name"
          placeholder="Nombre"
          defaultValue={filters.name}
          className="border rounded px-3 py-2"
          required
        />
        <input
          type="text"
          name="address"
          placeholder="Dirección"
          defaultValue={filters.address}
          className="border rounded px-3 py-2"
          required
        />
        <input
          type="number"
          name="minPrice"
          placeholder="Precio mínimo"
          defaultValue={filters.minPrice}
          className="border rounded px-3 py-2"
          required
        />
        <input
          type="number"
          name="maxPrice"
          placeholder="Precio máximo"
          defaultValue={filters.maxPrice}
          className="border rounded px-3 py-2"
          required
        />

        <button type="submit" className="px-4 py-2 bg-green-600 text-white rounded-lg">
          Filtrar
        </button>

        <ButtonMostrarTodos properties={properties} filters={filters} />
      </form>

      {/* Resultados */}
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
        {properties.map((p: any, i: number) => (
          <PropertyCard key={i} property={p} />
        ))}
      </div>
    </div>
  );
}