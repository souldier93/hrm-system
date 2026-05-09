import axios from 'axios'

const instance = axios.create({
    baseURL: 'http://localhost:5000/api',
    timeout: 10000
})

// Tự động gắn token vào mỗi request
instance.interceptors.request.use(config => {
    const token = localStorage.getItem('token')
    if (token) {
        config.headers.Authorization = `Bearer ${token}`
    }
    return config
})

// Tự động xử lý lỗi 401 (token hết hạn)
instance.interceptors.response.use(
    response => response,
    error => {
        if (error.response?.status === 401) {
            localStorage.removeItem('token')
            window.location.href = '/login'
        }
        return Promise.reject(error)
    }
)

export default instance