import api from './axios'

export default {
    getAll: (page = 1) => api.get(`/notification?page=${page}`),
    markAsRead: (id) => api.put(`/notification/${id}/read`),
    markAllAsRead: () => api.put('/notification/read-all'),
    broadcast: (data) => api.post('/notification/broadcast', data)
}