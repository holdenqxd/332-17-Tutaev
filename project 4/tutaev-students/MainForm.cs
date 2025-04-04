using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.Script.Serialization;
using System.Globalization;

namespace tutaev_students
{
    public partial class MainForm : Form
    {
        private List<Student> students = new List<Student>();
        private bool isDataChanged = false;
        private string currentFilePath = "students.json";
        private int editingIndex = -1;
        private JavaScriptSerializer serializer = new JavaScriptSerializer();

        public MainForm()
        {
            InitializeComponent();
            LoadData();
            SetupDataGridView();
            SetupFilters();
            InitializeCourseComboBox();
        }

        private void SetupDataGridView()
        {
            dataGridViewStudents.Columns.Add("LastName", "Фамилия");
            dataGridViewStudents.Columns.Add("FirstName", "Имя");
            dataGridViewStudents.Columns.Add("MiddleName", "Отчество");
            dataGridViewStudents.Columns.Add("Course", "Курс");
            dataGridViewStudents.Columns.Add("Group", "Группа");
            dataGridViewStudents.Columns.Add("BirthDate", "Дата рождения");
            dataGridViewStudents.Columns.Add("Email", "Email");

            dataGridViewStudents.AllowUserToAddRows = false;
            dataGridViewStudents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewStudents.MultiSelect = false;
        }

        private void SetupFilters()
        {
            cmbFilterCourse.Items.Add("Все курсы");
            cmbFilterGroup.Items.Add("Все группы");
            cmbFilterCourse.SelectedIndex = 0;
            cmbFilterGroup.SelectedIndex = 0;
        }

        private void InitializeCourseComboBox()
        {
            // Добавляем курсы от 1 до 4
            for (int i = 1; i <= 4; i++)
            {
                cmbCourse.Items.Add(i);
            }
            
            // Выбираем первый курс по умолчанию
            if (cmbCourse.Items.Count > 0)
            {
                cmbCourse.SelectedIndex = 0;
            }
        }

        private void LoadData()
        {
            if (File.Exists(currentFilePath))
            {
                try
                {
                    string json = File.ReadAllText(currentFilePath);
                    students = serializer.Deserialize<List<Student>>(json) ?? new List<Student>();
                    RefreshDataGridView();
                    UpdateFilters();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SaveData()
        {
            try
            {
                string json = serializer.Serialize(students);
                File.WriteAllText(currentFilePath, json);
                isDataChanged = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshDataGridView()
        {
            dataGridViewStudents.Rows.Clear();
            foreach (var student in students)
            {
                dataGridViewStudents.Rows.Add(
                    student.LastName,
                    student.FirstName,
                    student.MiddleName,
                    student.Course,
                    student.Group,
                    student.BirthDate.ToString("dd.MM.yyyy"),
                    student.Email
                );
            }
        }

        private void UpdateFilters()
        {
            var courses = students.Select(s => s.Course).Distinct().OrderBy(c => c).ToList();
            var groups = students.Select(s => s.Group).Distinct().OrderBy(g => g).ToList();

            cmbFilterCourse.Items.Clear();
            cmbFilterCourse.Items.Add("Все курсы");
            foreach (var course in courses)
            {
                cmbFilterCourse.Items.Add(course);
            }
            cmbFilterCourse.SelectedIndex = 0;

            cmbFilterGroup.Items.Clear();
            cmbFilterGroup.Items.Add("Все группы");
            foreach (var group in groups)
            {
                cmbFilterGroup.Items.Add(group);
            }
            cmbFilterGroup.SelectedIndex = 0;
        }

        private bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            string pattern = @"^[a-zA-Z0-9._%+-]{3,}@(yandex\.ru|gmail\.com|mail\.ru)$";
            return Regex.IsMatch(email, pattern);
        }

        private bool ValidateBirthDate(DateTime birthDate)
        {
            DateTime minDate = new DateTime(1992, 1, 1);
            DateTime maxDate = DateTime.Now;
            return birthDate >= minDate && birthDate <= maxDate;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                Student newStudent = new Student
                {
                    LastName = txtLastName.Text,
                    FirstName = txtFirstName.Text,
                    MiddleName = txtMiddleName.Text,
                    Course = (int)cmbCourse.SelectedItem,
                    Group = txtGroup.Text,
                    BirthDate = dateTimePickerBirthDate.Value,
                    Email = txtEmail.Text
                };

                students.Add(newStudent);
                RefreshDataGridView();
                UpdateFilters();
                ClearInputs();
                isDataChanged = true;
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewStudents.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите студента для редактирования", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ValidateInputs())
            {
                int index = dataGridViewStudents.SelectedRows[0].Index;
                students[index] = new Student
                {
                    LastName = txtLastName.Text,
                    FirstName = txtFirstName.Text,
                    MiddleName = txtMiddleName.Text,
                    Course = (int)cmbCourse.SelectedItem,
                    Group = txtGroup.Text,
                    BirthDate = dateTimePickerBirthDate.Value,
                    Email = txtEmail.Text
                };

                RefreshDataGridView();
                UpdateFilters();
                ClearInputs();
                isDataChanged = true;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewStudents.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите студента для удаления", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранного студента?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int index = dataGridViewStudents.SelectedRows[0].Index;
                students.RemoveAt(index);
                RefreshDataGridView();
                UpdateFilters();
                ClearInputs();
                isDataChanged = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveData();
            MessageBox.Show("Данные успешно сохранены", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                        {
                            sw.WriteLine("Фамилия,Имя,Отчество,Курс,Группа,Дата рождения,Email");
                            foreach (var student in students)
                            {
                                sw.WriteLine($"{student.LastName},{student.FirstName},{student.MiddleName}," +
                                    $"{student.Course},{student.Group},{student.BirthDate:dd.MM.yyyy},{student.Email}");
                            }
                        }
                        MessageBox.Show("Данные успешно экспортированы", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при экспорте данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string json = File.ReadAllText(openFileDialog.FileName);
                        var importedStudents = serializer.Deserialize<List<Student>>(json);

                        if (importedStudents != null && importedStudents.Count > 0)
                        {
                            // Добавляем импортированных студентов к существующим
                            students.AddRange(importedStudents);
                            RefreshDataGridView();
                            SaveData();
                            MessageBox.Show($"Успешно импортировано {importedStudents.Count} студентов.", "Импорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Файл не содержит данных о студентах.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при импорте файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtMiddleName.Text) ||
                string.IsNullOrWhiteSpace(txtGroup.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Все поля обязательны для заполнения!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbCourse.SelectedItem == null)
            {
                MessageBox.Show("Выберите курс!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCourse.Focus();
                return false;
            }

            if (!ValidateEmail(txtEmail.Text))
            {
                MessageBox.Show("Некорректный формат email. Используйте домены: yandex.ru, gmail.com, mail.ru", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return false;
            }

            if (!ValidateBirthDate(dateTimePickerBirthDate.Value))
            {
                MessageBox.Show("Дата рождения должна быть не ранее 01.01.1992 и не позднее текущей даты", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dateTimePickerBirthDate.Focus();
                return false;
            }

            return true;
        }

        private void ClearInputs()
        {
            txtLastName.Clear();
            txtFirstName.Clear();
            txtMiddleName.Clear();
            cmbCourse.SelectedIndex = 0;
            txtGroup.Clear();
            dateTimePickerBirthDate.Value = DateTime.Now;
            txtEmail.Clear();
            dataGridViewStudents.ClearSelection();
        }

        private void dataGridViewStudents_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewStudents.SelectedRows.Count > 0)
            {
                int index = dataGridViewStudents.SelectedRows[0].Index;
                var student = students[index];

                txtLastName.Text = student.LastName;
                txtFirstName.Text = student.FirstName;
                txtMiddleName.Text = student.MiddleName;
                cmbCourse.SelectedItem = student.Course;
                txtGroup.Text = student.Group;
                dateTimePickerBirthDate.Value = student.BirthDate;
                txtEmail.Text = student.Email;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.ToLower();
            var filteredStudents = students.Where(s => 
                s.LastName.ToLower().Contains(searchText)).ToList();
            RefreshDataGridViewWithFilter(filteredStudents);
        }

        private void cmbFilterCourse_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void cmbFilterGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filteredStudents = students.AsQueryable();

            if (cmbFilterCourse.SelectedItem != null && cmbFilterCourse.SelectedItem.ToString() != "Все курсы")
            {
                int course = (int)cmbFilterCourse.SelectedItem;
                filteredStudents = filteredStudents.Where(s => s.Course == course);
            }

            if (cmbFilterGroup.SelectedItem != null && cmbFilterGroup.SelectedItem.ToString() != "Все группы")
            {
                string group = cmbFilterGroup.SelectedItem.ToString();
                filteredStudents = filteredStudents.Where(s => s.Group == group);
            }

            string searchText = txtSearch.Text.ToLower();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredStudents = filteredStudents.Where(s => s.LastName.ToLower().Contains(searchText));
            }

            var result = filteredStudents.ToList();
            RefreshDataGridViewWithFilter(result);
        }

        private void RefreshDataGridViewWithFilter(List<Student> filteredStudents)
        {
            dataGridViewStudents.Rows.Clear();
            foreach (var student in filteredStudents)
            {
                dataGridViewStudents.Rows.Add(
                    student.LastName,
                    student.FirstName,
                    student.MiddleName,
                    student.Course,
                    student.Group,
                    student.BirthDate.ToString("dd.MM.yyyy"),
                    student.Email
                );
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isDataChanged)
            {
                var result = MessageBox.Show("Есть несохраненные изменения. Сохранить?", 
                    "Предупреждение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    SaveData();
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }

    public class Student
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public int Course { get; set; }
        public string Group { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
    }
} 