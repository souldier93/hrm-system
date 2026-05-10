<template>
  <div>
    <!-- Thẻ tổng quan -->
    <el-row :gutter="16" style="margin-bottom:20px">
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card" style="color:#409EFF">
            <el-icon size="32"><User /></el-icon>
            <div class="stat-number">{{ summary.totalEmployees }}</div>
            <div class="stat-label">Nhân viên đang làm</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card" style="color:#67C23A">
            <el-icon size="32"><Calendar /></el-icon>
            <div class="stat-number">{{ summary.checkedInToday }}</div>
            <div class="stat-label">Check in hôm nay</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card" style="color:#E6A23C">
            <el-icon size="32"><Warning /></el-icon>
            <div class="stat-number">{{ summary.lateToday }}</div>
            <div class="stat-label">Đi muộn hôm nay</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card" style="color:#F56C6C">
            <el-icon size="32"><Money /></el-icon>
            <div class="stat-number">
              {{ formatMoney(summary.totalSalaryThisMonth) }}
            </div>
            <div class="stat-label">Tổng lương tháng này</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Biểu đồ hàng 1 -->
    <el-row :gutter="16" style="margin-bottom:20px">
      <!-- Pie chart: NV theo phòng ban -->
      <el-col :span="10">
        <el-card>
          <template #header>👥 Nhân viên theo phòng ban</template>
          <Pie v-if="pieData" :data="pieData" :options="pieOptions" />
        </el-card>
      </el-col>

      <!-- Bar chart: Chấm công theo tháng -->
      <el-col :span="14">
        <el-card>
          <template #header>
            📅 Chấm công năm {{ selectedYear }}
            <el-input-number
              v-model="selectedYear"
              size="small"
              style="float:right; width:100px"
              @change="loadCharts"
            />
          </template>
          <Bar v-if="barData" :data="barData" :options="barOptions" />
        </el-card>
      </el-col>
    </el-row>

    <!-- Biểu đồ hàng 2 -->
    <el-row :gutter="16">
      <!-- Line chart: Tổng lương theo tháng -->
      <el-col :span="14">
        <el-card>
          <template #header>💰 Tổng lương theo tháng</template>
          <Line v-if="lineData" :data="lineData" :options="lineOptions" />
        </el-card>
      </el-col>

      <!-- Top 5 chuyên cần -->
      <el-col :span="10">
        <el-card>
          <template #header>🏆 Top 5 nhân viên chuyên cần</template>
          <el-table :data="topAttendance" stripe>
            <el-table-column type="index" label="#" width="40" />
            <el-table-column prop="name" label="Họ tên" />
            <el-table-column prop="workDays" label="Ngày công" width="90">
              <template #default="{ row }">
                <el-tag type="success">{{ row.workDays }} ngày</el-tag>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { Pie, Bar, Line } from 'vue-chartjs'
import {
  Chart as ChartJS, ArcElement, Tooltip, Legend,
  CategoryScale, LinearScale, BarElement, PointElement,
  LineElement, Title
} from 'chart.js'
import dashboardApi from '../api/dashboard'

ChartJS.register(
  ArcElement, Tooltip, Legend,
  CategoryScale, LinearScale, BarElement,
  PointElement, LineElement, Title
)

const summary = ref({
  totalEmployees: 0, checkedInToday: 0,
  lateToday: 0, totalSalaryThisMonth: 0
})
const pieData = ref(null)
const barData = ref(null)
const lineData = ref(null)
const topAttendance = ref([])
const selectedYear = ref(new Date().getFullYear())

const pieOptions = { responsive: true, plugins: { legend: { position: 'bottom' } } }
const barOptions = {
  responsive: true,
  plugins: { legend: { position: 'top' } },
  scales: { x: { stacked: false }, y: { stacked: false } }
}
const lineOptions = {
  responsive: true,
  plugins: { legend: { position: 'top' } }
}

const COLORS = [
  '#409EFF','#67C23A','#E6A23C','#F56C6C',
  '#909399','#9B59B6','#1ABC9C'
]
const MONTHS = ['T1','T2','T3','T4','T5','T6','T7','T8','T9','T10','T11','T12']

function formatMoney(value) {
  return new Intl.NumberFormat('vi-VN',
    { style: 'currency', currency: 'VND' }).format(value || 0)
}

async function loadSummary() {
  const res = await dashboardApi.getSummary()
  summary.value = res.data
}

async function loadCharts() {
  const [deptRes, attendRes, salaryRes, topRes] = await Promise.all([
    dashboardApi.getEmployeesByDepartment(),
    dashboardApi.getAttendanceByMonth(selectedYear.value),
    dashboardApi.getSalaryByMonth(selectedYear.value),
    dashboardApi.getTopAttendance(
      new Date().getMonth() + 1, selectedYear.value)
  ])

  // Pie chart
  pieData.value = {
    labels: deptRes.data.map(d => d.department),
    datasets: [{
      data: deptRes.data.map(d => d.count),
      backgroundColor: COLORS
    }]
  }

  // Bar chart
  barData.value = {
    labels: MONTHS,
    datasets: [
      {
        label: 'Đúng giờ',
        data: MONTHS.map((_, i) => {
          const found = attendRes.data.find(d => d.month === i + 1)
          return found?.onTime || 0
        }),
        backgroundColor: '#67C23A'
      },
      {
        label: 'Muộn',
        data: MONTHS.map((_, i) => {
          const found = attendRes.data.find(d => d.month === i + 1)
          return found?.late || 0
        }),
        backgroundColor: '#E6A23C'
      }
    ]
  }

  // Line chart
  lineData.value = {
    labels: MONTHS,
    datasets: [{
      label: 'Tổng lương (VND)',
      data: MONTHS.map((_, i) => {
        const found = salaryRes.data.find(d => d.month === i + 1)
        return found?.total || 0
      }),
      borderColor: '#409EFF',
      backgroundColor: 'rgba(64,158,255,0.1)',
      fill: true,
      tension: 0.4
    }]
  }

  topAttendance.value = topRes.data
}

onMounted(async () => {
  await Promise.all([loadSummary(), loadCharts()])
})
</script>

<style scoped>
.stat-card {
  text-align: center;
  padding: 10px;
}
.stat-number {
  font-size: 28px;
  font-weight: bold;
  margin: 8px 0 4px;
}
.stat-label {
  color: #999;
  font-size: 13px;
}
</style>