<template>
  <div>
    <!-- Thanh công cụ -->
    <el-card style="margin-bottom:16px">
      <el-row justify="space-between" align="middle">
        <el-col :span="12">
          <el-input
            v-model="search"
            placeholder="Tìm kiếm nhân viên..."
            clearable
            style="width:300px"
          >
            <template #prefix><el-icon><Search /></el-icon></template>
          </el-input>
        </el-col>
        <el-col :span="12" style="text-align:right">
          <el-button type="primary" @click="openDialog()">
            <el-icon><Plus /></el-icon> Thêm nhân viên
          </el-button>
        </el-col>
      </el-row>
    </el-card>

    <!-- Bảng danh sách -->
    <el-card>
      <el-table :data="filteredEmployees" stripe v-loading="loading">
        <el-table-column prop="id" label="ID" width="60" />
        <el-table-column prop="fullName" label="Họ tên" min-width="150" />
        <el-table-column prop="phone" label="Số điện thoại" width="130" />
        <el-table-column prop="email" label="Email" min-width="180" />
        <el-table-column prop="departmentName" label="Phòng ban" width="140" />
        <el-table-column prop="positionName" label="Chức vụ" width="130" />
        <el-table-column prop="joinDate" label="Ngày vào" width="110" />
        <el-table-column prop="status" label="Trạng thái" width="110">
          <template #default="{ row }">
            <el-tag :type="row.status === 'Active' ? 'success' : 'danger'">
              {{ row.status === 'Active' ? 'Đang làm' : 'Nghỉ việc' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="Thao tác" width="130" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="openDialog(row)">Sửa</el-button>
            <el-button size="small" type="danger" @click="handleDelete(row.id)">Xóa</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- Dialog thêm/sửa -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEditing ? 'Sửa nhân viên' : 'Thêm nhân viên'"
      width="500px"
    >
      <el-form :model="form" label-width="120px">
        <el-form-item label="Họ tên">
          <el-input v-model="form.fullName" />
        </el-form-item>
        <el-form-item label="Số điện thoại">
          <el-input v-model="form.phone" />
        </el-form-item>
        <el-form-item label="Email">
          <el-input v-model="form.email" />
        </el-form-item>
        <el-form-item label="Ngày vào làm">
          <el-date-picker
            v-model="form.joinDate"
            type="date"
            format="YYYY-MM-DD"
            value-format="YYYY-MM-DD"
            style="width:100%"
          />
        </el-form-item>
        <el-form-item label="Phòng ban">
          <el-select v-model="form.departmentId" style="width:100%">
            <el-option
              v-for="d in departments"
              :key="d.id"
              :label="d.name"
              :value="d.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Chức vụ">
          <el-select v-model="form.positionId" style="width:100%">
            <el-option
              v-for="p in positions"
              :key="p.id"
              :label="p.name"
              :value="p.id"
            />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">Hủy</el-button>
        <el-button type="primary" :loading="saving" @click="handleSave">
          {{ isEditing ? 'Cập nhật' : 'Thêm mới' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import employeeApi from '../api/employee'
import api from '../api/axios'

const employees = ref([])
const departments = ref([])
const positions = ref([])
const loading = ref(false)
const saving = ref(false)
const search = ref('')
const dialogVisible = ref(false)
const isEditing = ref(false)
const editId = ref(null)

const form = ref({
  fullName: '', phone: '', email: '',
  joinDate: '', departmentId: null, positionId: null
})

const filteredEmployees = computed(() =>
  employees.value.filter(e =>
    e.fullName.toLowerCase().includes(search.value.toLowerCase()) ||
    e.email.toLowerCase().includes(search.value.toLowerCase())
  )
)

async function loadData() {
  loading.value = true
  try {
    const [empRes, depRes, posRes] = await Promise.all([
      employeeApi.getAll(),
      api.get('/department'),
      api.get('/position')
    ])
    employees.value = empRes.data
    departments.value = depRes.data
    positions.value = posRes.data
  } finally {
    loading.value = false
  }
}

function openDialog(row = null) {
  if (row) {
    isEditing.value = true
    editId.value = row.id
    form.value = {
      fullName: row.fullName,
      phone: row.phone,
      email: row.email,
      joinDate: row.joinDate,
      departmentId: departments.value.find(d => d.name === row.departmentName)?.id,
      positionId: positions.value.find(p => p.name === row.positionName)?.id
    }
  } else {
    isEditing.value = false
    editId.value = null
    form.value = {
      fullName: '', phone: '', email: '',
      joinDate: '', departmentId: null, positionId: null
    }
  }
  dialogVisible.value = true
}

async function handleSave() {
  saving.value = true
  try {
    if (isEditing.value) {
      await employeeApi.update(editId.value, form.value)
      ElMessage.success('Cập nhật thành công!')
    } else {
      await employeeApi.create(form.value)
      ElMessage.success('Thêm mới thành công!')
    }
    dialogVisible.value = false
    await loadData()
  } catch {
    ElMessage.error('Có lỗi xảy ra!')
  } finally {
    saving.value = false
  }
}

async function handleDelete(id) {
  await ElMessageBox.confirm('Bạn có chắc muốn xóa nhân viên này?', 'Xác nhận', {
    type: 'warning'
  })
  try {
    await employeeApi.delete(id)
    ElMessage.success('Đã xóa thành công!')
    await loadData()
  } catch {
    ElMessage.error('Có lỗi xảy ra!')
  }
}

onMounted(loadData)
</script>