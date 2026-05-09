import { createRouter, createWebHistory } from 'vue-router'
import store from '../store'
import AppLayout from '../components/AppLayout.vue'

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/login',
            name: 'Đăng nhập',
            component: () => import('../views/LoginView.vue'),
            meta: { guest: true }
        },
        {
            path: '/',
            component: AppLayout,
            meta: { requiresAuth: true },
            children: [
                {
                    path: '',
                    name: 'Trang chủ',
                    component: () => import('../views/HomeView.vue')
                },
                {
                    path: 'employees',
                    name: 'Nhân viên',
                    component: () => import('../views/EmployeeView.vue')
                },
                {
                    path: 'attendance',
                    name: 'Chấm công',
                    component: () => import('../views/AttendanceView.vue')
                },
                {
                    path: 'salary',
                    name: 'Lương',
                    component: () => import('../views/SalaryView.vue')
                }
            ]
        }
    ]
})

router.beforeEach((to, from, next) => {
    if (to.meta.requiresAuth && !store.getters.isLoggedIn) {
        next('/login')
    } else if (to.meta.guest && store.getters.isLoggedIn) {
        next('/')
    } else {
        next()
    }
})

export default router