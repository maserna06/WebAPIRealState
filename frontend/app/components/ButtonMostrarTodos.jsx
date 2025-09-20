'use client';

export default function ButtonMostrarTodos({ properties, filters }) {
  const handleClick = () => {
    window.location.href = window.location.origin;
  }

  return (
    <button type="reset" onClick={handleClick} className="px-4 py-2 bg-blue-600 text-white rounded-lg">
      Mostrar todas
    </button>
  );
}
