import api from './axios'

export default {
    getByMonth: (month, year) =>
        api.get(`/salary?month=${month}&year=${year}`),
    calculate: (month, year) =>
        api.post(`/salary/calculate?month=${month}&year=${year}`),
    update: (id, data) => api.put(`/salary/${id}`, data),

    // Thêm dòng này
    exportExcel: (month, year) =>
        api.get(`/salary/export?month=${month}&year=${year}`,
            { responseType: 'blob' })
}