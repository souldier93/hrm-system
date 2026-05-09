<template>
  <div>
    <!-- Card chấm công cá nhân -->
    <el-card style="margin-bottom:16px">
      <el-row align="middle" :gutter="20">
        <el-col :span="12">
          <div style="font-size:16px; font-weight:bold; margin-bottom:8px">
            📅 Hôm nay: {{ today }}
          </div>
          <div v-if="todayRecord">
            <el-tag type="success">
              Vào: {{ todayRecord.checkIn || '--' }}
            </el-tag>
            <el-tag type="info" style="margin-left:8px">
              Ra: {{ todayRecord.checkOut || 'Chưa check out' }}
            </el-tag>
            <el-tag
              :type="todayRecord.status === 'OnTime' ? 'success' : 'warning'"
              style="margin-left:8px"
            >
              {{ todayRecord.status === 'OnTime' ? 'Đúng giờ' : 'Muộn' }}
            </el-tag>
          </div>
          <div v-else style="color:#999">Chưa check in hôm nay</div>
        </el-col>
        <el-col :span="12" style="text-align:right">
          <el-button
            type="success"
            size="large"
            :disabled="!!todayRecord?.checkIn"
            @click="handleCheckIn"
          >
            ✅ Check In
          </el-button>
          <el-button
            type="warning"
            size="large"
            :disabled="!todayRecord?.checkIn || !!todayRecord?.checkOut"
            style="margin-left:10px"
            @click="handleCheckOut"
          >
            🚪 Check Out
          </el-button>
        </el-col>
      </el-row>
    </el-card>

    <!-- Bảng lịch sử chấm công (Admin) -->
    <el-card v-if="isAdmin">
      <el-row style="margin-bottom:16px" :gutter="10">
        <el-col :span="6">
          <el-select v-model="selectedMonth" @change="loadHistory">
            <el-option v-for="m in 12" :key="m" :label="`Tháng ${m}`" :value="m" />
          </el-select>
        </el-col>
        <el-col :span="6">
          <el-input-number v-model="selectedYear" @change="loadHistory" />
        </el-col>
      </el-row>

      <el-table :data="history" stripe v-loading="loading">
        <el-table-column prop="employeeName" label="Nhân viên" min-width="140" />
        <el-table-column prop="date" label="Ngày" width="110" />
        <el-table-column prop="checkIn" label="Giờ vào" width="90" />
        <el-table-column prop="checkOut" label="Giờ ra" width="90" />
        <el-table-column prop="status" label="Trạng thái" width="110">
          <template #default="{ row }">
            <el-tag :type="row.status === 'OnTime' ? 'success' : row.status === 'Late' ? 'warning' : 'danger'">
              {{ row.status === 'OnTime' ? 'Đúng giờ' : row.status === 'Late' ? 'Muộn' : 'Vắng' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="notes" label="Ghi chú" min-width="150" />
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { useStore } from 'vuex'
import attendanceApi from '../api/attendance'

const store = useStore()
const isAdmin = computed(() => store.getters.isAdmin)

const today = new Date().toLocaleDateString('vi-VN')
const todayRecord = ref(null)
const history = ref([])
const loading = ref(false)
const selectedMonth = ref(new Date().getMonth() + 1)
const selectedYear = ref(new Date().getFullYear())

async function loadToday() {
  try {
    const res = await attendanceApi.getToday()
    todayRecord.value = res.data
  } catch {
    todayRecord.value = null
  }
}

async function loadHistory() {
  if (!isAdmin.value) return
  loading.value = true
  try {
    const res = await attendanceApi.getByMonth(selectedMonth.value, selectedYear.value)
    history.value = res.data
  } finally {
    loading.value = false
  }
}

async function handleCheckIn() {
  try {
    await attendanceApi.checkIn()
    ElMessage.success('Check in thành công!')
    await loadToday()
    await loadHistory()
  } catch (err) {
    ElMessage.error(err.response?.data?.message || 'Có lỗi xảy ra!')
  }
}

async function handleCheckOut() {
  try {
    await attendanceApi.checkOut()
    ElMessage.success('Check out thành công!')
    await loadToday()
    await loadHistory()
  } catch (err) {
    ElMessage.error(err.response?.data?.message || 'Có lỗi xảy ra!')
  }
}

onMounted(async () => {
  await loadToday()
  await loadHistory()
})
</script>