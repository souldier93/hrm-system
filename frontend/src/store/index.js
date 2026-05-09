import { createStore } from 'vuex'
import api from '../api/axios'

export default createStore({
    state: {
        token: localStorage.getItem('token') || null,
        user: JSON.parse(localStorage.getItem('user') || 'null')
    },
    getters: {
        isLoggedIn: state => !!state.token,
        currentUser: state => state.user,
        isAdmin: state => state.user?.role === 'Admin'
    },
    mutations: {
        SET_AUTH(state, { token, user }) {
            state.token = token
            state.user = user
            localStorage.setItem('token', token)
            localStorage.setItem('user', JSON.stringify(user))
        },
        LOGOUT(state) {
            state.token = null
            state.user = null
            localStorage.removeItem('token')
            localStorage.removeItem('user')
        }
    },
    actions: {
        async login({ commit }, credentials) {
            const res = await api.post('/auth/login', credentials)
            commit('SET_AUTH', {
                token: res.data.token,
                user: {
                    fullName: res.data.fullName,
                    role: res.data.role
                }
            })
        },
        logout({ commit }) {
            commit('LOGOUT')
        }
    }
})