<template>
  <div>
    <el-row :gutter="20">
      <el-col :span="8">
        <el-card>
          <template #header>👥 Nhân viên</template>
          <div style="font-size:32px; font-weight:bold; color:#409EFF">
            {{ stats.employees }}
          </div>
          <div style="color:#999">Đang làm việc</div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card>
          <template #header>📅 Chấm công hôm nay</template>
          <div style="font-size:32px; font-weight:bold; color:#67C23A">
            {{ stats.attendanceToday }}
          </div>
          <div style="color:#999">Đã check in</div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card>
          <template #header>💰 Lương tháng này</template>
          <div style="font-size:32px; font-weight:bold; color:#E6A23C">
            {{ stats.salaryMonth }}
          </div>
          <div style="color:#999">Đã tính lương</div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useSignalR } from '../composables/useSignalR'
import { useStore } from 'vuex'
import { onMounted, onUnmounted } from 'vue'

const store = useStore()
const { connect, disconnect } = useSignalR()

onMounted(() => {
    const token = store.state.token
    if (token) connect(token)
})

onUnmounted(() => disconnect())

const stats = ref({
  employees: 1,
  attendanceToday: 0,
  salaryMonth: 0
})
</script>