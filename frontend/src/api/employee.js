import api from './axios'

export default {
    getAll: () => api.get('/employee'),
    getById: (id) => api.get(`/employee/${id}`),
    create: (data) => api.post('/employee', data),
    update: (id, data) => api.put(`/employee/${id}`, data),
    delete: (id) => api.delete(`/employee/${id}`)
}