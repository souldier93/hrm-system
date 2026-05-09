<template>
  <div>
    <!-- Bộ lọc + nút tính lương -->
    <el-card style="margin-bottom: 16px">
      <el-row align="middle" :gutter="10">
        <el-col :span="4">
          <el-select v-model="selectedMonth" @change="loadSalaries">
            <el-option v-for="m in 12" :key="m" :label="`Tháng ${m}`" :value="m" />
          </el-select>
        </el-col>
        <el-col :span="4">
          <el-button @click="handleExport" :loading="exporting"> 📥 Xuất Excel </el-button>
        </el-col>

        <el-col :span="4">
          <el-input-number v-model="selectedYear" @change="loadSalaries" />
        </el-col>
        <el-col :span="16" style="text-align: right">
          <el-button type="primary" :loading="calculating" @click="handleCalculate">
            ⚡ Tính lương tháng {{ selectedMonth }}/{{ selectedYear }}
          </el-button>
        </el-col>
      </el-row>
    </el-card>

    <!-- Bảng lương -->
    <el-card>
      <el-table :data="salaries" stripe v-loading="loading">
        <el-table-column prop="employeeName" label="Nhân viên" min-width="140" />
        <el-table-column prop="positionName" label="Chức vụ" width="140" />
        <el-table-column prop="workDays" label="Ngày công" width="100" />
        <el-table-column prop="baseSalary" label="Lương cơ bản" width="140">
          <template #default="{ row }">
            {{ formatMoney(row.baseSalary) }}
          </template>
        </el-table-column>
        <el-table-column prop="allowance" label="Phụ cấp" width="120">
          <template #default="{ row }">
            {{ formatMoney(row.allowance) }}
          </template>
        </el-table-column>
        <el-table-column prop="advance" label="Tạm ứng" width="120">
          <template #default="{ row }">
            {{ formatMoney(row.advance) }}
          </template>
        </el-table-column>
        <el-table-column prop="total" label="Thực lĩnh" width="140">
          <template #default="{ row }">
            <span style="color: #67c23a; font-weight: bold">
              {{ formatMoney(row.total) }}
            </span>
          </template>
        </el-table-column>
        <el-table-column label="Thao tác" width="100" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="openEdit(row)">Sửa</el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- Tổng cộng -->
      <div style="text-align: right; margin-top: 16px; font-size: 16px">
        <strong>Tổng chi lương: </strong>
        <span style="color: #e6a23c; font-size: 18px; font-weight: bold">
          {{ formatMoney(totalSalary) }}
        </span>
      </div>
    </el-card>

    <!-- Dialog sửa lương -->
    <el-dialog v-model="editVisible" title="Chỉnh sửa lương" width="400px">
      <el-form :model="editForm" label-width="100px">
        <el-form-item label="Phụ cấp">
          <el-input-number v-model="editForm.allowance" :min="0" style="width: 100%" />
        </el-form-item>
        <el-form-item label="Tạm ứng">
          <el-input-number v-model="editForm.advance" :min="0" style="width: 100%" />
        </el-form-item>
        <el-form-item label="Thực lĩnh">
          <el-input-number v-model="editForm.total" :min="0" style="width: 100%" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="editVisible = false">Hủy</el-button>
        <el-button type="primary" @click="handleUpdate">Lưu</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import salaryApi from '../api/salary'

const salaries = ref([])
const loading = ref(false)
const calculating = ref(false)
const selectedMonth = ref(new Date().getMonth() + 1)
const selectedYear = ref(new Date().getFullYear())
const editVisible = ref(false)
const editId = ref(null)
const editForm = ref({ allowance: 0, advance: 0, total: 0 })

const exporting = ref(false)

async function handleExport() {
  exporting.value = true
  try {
    const res = await salaryApi.exportExcel(selectedMonth.value, selectedYear.value)
    const url = window.URL.createObjectURL(new Blob([res.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', `BangLuong_T${selectedMonth.value}_${selectedYear.value}.xlsx`)
    document.body.appendChild(link)
    link.click()
    link.remove()
  } catch {
    ElMessage.error('Có lỗi khi xuất Excel!')
  } finally {
    exporting.value = false
  }
}

const totalSalary = computed(() => salaries.value.reduce((sum, s) => sum + s.total, 0))

function formatMoney(value) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value)
}

async function loadSalaries() {
  loading.value = true
  try {
    const res = await salaryApi.getByMonth(selectedMonth.value, selectedYear.value)
    salaries.value = res.data
  } finally {
    loading.value = false
  }
}

async function handleCalculate() {
  calculating.value = true
  try {
    const res = await salaryApi.calculate(selectedMonth.value, selectedYear.value)
    ElMessage.success(res.data.message)
    await loadSalaries()
  } catch {
    ElMessage.error('Có lỗi xảy ra!')
  } finally {
    calculating.value = false
  }
}

function openEdit(row) {
  editId.value = row.id
  editForm.value = {
    allowance: row.allowance,
    advance: row.advance,
    total: row.total,
  }
  editVisible.value = true
}

async function handleUpdate() {
  try {
    await salaryApi.update(editId.value, editForm.value)
    ElMessage.success('Cập nhật thành công!')
    editVisible.value = false
    await loadSalaries()
  } catch {
    ElMessage.error('Có lỗi xảy ra!')
  }
}

onMounted(loadSalaries)
</script>
