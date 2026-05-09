import api from './axios'

export default {
    getByMonth: (month, year) =>
        api.get(`/attendance?month=${month}&year=${year}`),
    getToday: () => api.get('/attendance/today'),
    checkIn: () => api.post('/attendance/checkin'),
    checkOut: () => api.post('/attendance/checkout')
}