import api from './axios'

export default {
    getSummary: () => api.get('/dashboard/summary'),
    getEmployeesByDepartment: () =>
        api.get('/dashboard/employees-by-department'),
    getAttendanceByMonth: (year) =>
        api.get(`/dashboard/attendance-by-month?year=${year}`),
    getSalaryByMonth: (year) =>
        api.get(`/dashboard/salary-by-month?year=${year}`),
    getTopAttendance: (month, year) =>
        api.get(`/dashboard/top-attendance?month=${month}&year=${year}`)
}