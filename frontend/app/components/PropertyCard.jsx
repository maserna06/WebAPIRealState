'use client';

export default function PropertyCard({ property }) {
  return (
    <div className="border rounded-lg shadow p-4">
      <img src={property.images[0].file} alt={property.name} className="w-full h-40 object-cover rounded" />
      <h2 className="text-lg font-bold mt-2">{property.name}</h2>
      <p>{property.address}</p>
      <p className="font-semibold">${property.price}</p>
    </div>
  );
}
