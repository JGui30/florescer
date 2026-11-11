import { Routes, Route } from 'react-router-dom';
import ClientApp from './client/ClientApp.jsx';
import AdminApp from './admin/AdminApp.jsx';

export default function App() {
  return (
    <Routes>
      {}
      <Route path="/*" element={<ClientApp />} />

      {}
      <Route path="/admin/*" element={<AdminApp />} />
    </Routes>
  );
}
