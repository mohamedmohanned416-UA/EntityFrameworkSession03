using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Data;
using OnlineLearningPlatform.Models;

namespace OnlineLearningPlatform;

public class Program
{
    public static void Main()
    {
        try
        {
            using var db = new LearningDbContext();
            db.Database.Migrate();
            SeedStudentsAndEnrollments(db);
            AddInstructor(db);
            AddCourse(db);
            AddSeveralStudents(db);
            EnrollStudent(db);
            FindCourse(db);
            FilterCourses(db);
            PrintCategories(db);
            CourseRoster(db);
            ChangePrice(db);
            UpdateProgress(db);
            PublishCourse(db);
            Unenroll(db);
            DeleteCategoryWithException(db);
            DeleteCategorySafely(db);
            DeleteCourse(db);
            BulkPriceRaise(db);
            BulkDeleteDropped(db);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application error: {ex.GetBaseException().Message}");
        }
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine($"\n========== {title} ==========");
    }

    private static void SeedStudentsAndEnrollments(LearningDbContext db)
    {
        PrintSection("Seed students and enrollments");
        if (!db.Students.Any())
        {
            db.Students.AddRange(
                new Student { FullName = "Adam Ali", Email = "adam@student.com", DateOfBirth = new DateTime(2002, 3, 4) },
                new Student { FullName = "Laila Samir", Email = "laila@student.com", DateOfBirth = new DateTime(2001, 6, 8) },
                new Student { FullName = "Youssef Adel", Email = "youssef@student.com", DateOfBirth = new DateTime(2003, 9, 13) },
                new Student { FullName = "Hana Mahmoud", Email = "hana@student.com", DateOfBirth = new DateTime(2000, 1, 25) },
                new Student { FullName = "Karim Ashraf", Email = "karim@student.com", DateOfBirth = new DateTime(2002, 12, 2) }
            );
            db.SaveChanges();
            Console.WriteLine("Added 5 students.");
        }
        else
        {
            Console.WriteLine("Students already exist. Skipped student seed.");
        }

        if (!db.Enrollments.Any())
        {
            var emails = new[] { "adam@student.com", "laila@student.com", "youssef@student.com", "hana@student.com", "karim@student.com" };
            var students = emails.Select(email => db.Students.FirstOrDefault(s => s.Email == email)).ToArray();
            var courseIds = new[] { 1, 2, 4, 5 };
            if (students.Any(s => s == null) || courseIds.Any(id => !db.Courses.Any(c => c.Id == id && c.IsPublished)))
            {
                Console.WriteLine("Enrollment seed skipped: required students or published courses are missing.");
                return;
            }
            var ids = students.Select(s => s!.Id).ToArray();
            var date = new DateTime(2025, 1, 10);
            db.Enrollments.AddRange(
                new Enrollment { StudentId = ids[0], CourseId = 1, EnrolledAt = date, ProgressPercentage = 30, Status = EnrollmentStatus.Active },
                new Enrollment { StudentId = ids[1], CourseId = 1, EnrolledAt = date, ProgressPercentage = 100, Status = EnrollmentStatus.Completed, FinalGrade = 95m },
                new Enrollment { StudentId = ids[2], CourseId = 1, EnrolledAt = date, ProgressPercentage = 15, Status = EnrollmentStatus.Dropped },
                new Enrollment { StudentId = ids[3], CourseId = 2, EnrolledAt = date, ProgressPercentage = 50, Status = EnrollmentStatus.Active },
                new Enrollment { StudentId = ids[4], CourseId = 2, EnrolledAt = date, ProgressPercentage = 100, Status = EnrollmentStatus.Completed, FinalGrade = 88m },
                new Enrollment { StudentId = ids[0], CourseId = 4, EnrolledAt = date, ProgressPercentage = 60, Status = EnrollmentStatus.Active },
                new Enrollment { StudentId = ids[1], CourseId = 4, EnrolledAt = date, ProgressPercentage = 20, Status = EnrollmentStatus.Dropped },
                new Enrollment { StudentId = ids[2], CourseId = 5, EnrolledAt = date, ProgressPercentage = 10, Status = EnrollmentStatus.Active }
            );
            db.SaveChanges();
            Console.WriteLine("Added 8 enrollments covering all statuses.");
        }
        else
        {
            Console.WriteLine("Enrollments already exist. Skipped enrollment seed.");
        }
    }

    private static void AddInstructor(LearningDbContext db)
    {
        PrintSection("Add instructor");
        var instructor = new Instructor
        {
            FullName = "Alex Parker",
            Email = $"alex.{Guid.NewGuid():N}@learn.com",
            Bio = "Cloud instructor",
            JoinedAt = DateTime.UtcNow
        };
        db.Instructors.Add(instructor);
        db.SaveChanges();
        Console.WriteLine($"Created instructor Id: {instructor.Id}");
    }

    private static void AddCourse(LearningDbContext db)
    {
        PrintSection("Add course");
        int instructorId = 2;
        int categoryId = 4;
        if (!db.Instructors.Any(i => i.Id == instructorId))
        {
            Console.WriteLine("Instructor not found.");
            return;
        }
        if (!db.Categories.Any(c => c.Id == categoryId))
        {
            Console.WriteLine("Category not found.");
            return;
        }
        var course = new Course
        {
            Title = $"JavaScript Essentials {Guid.NewGuid():N}"[..30],
            Description = "JavaScript for beginners",
            Price = 350m,
            Level = CourseLevel.Beginner,
            IsPublished = false,
            InstructorId = instructorId,
            CategoryId = categoryId
        };
        db.Courses.Add(course);
        db.SaveChanges();
        Console.WriteLine($"Created course Id: {course.Id}");
    }

    private static void AddSeveralStudents(LearningDbContext db)
    {
        PrintSection("Add 3 students");
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var students = new[]
        {
            new Student { FullName = "Nour Ahmed", Email = $"nour.{suffix}@student.com", DateOfBirth = new DateTime(2003, 2, 3) },
            new Student { FullName = "Salma Ali", Email = $"salma.{suffix}@student.com", DateOfBirth = new DateTime(2002, 5, 6) },
            new Student { FullName = "Ziad Tarek", Email = $"ziad.{suffix}@student.com", DateOfBirth = new DateTime(2001, 8, 9) }
        };
        db.Students.AddRange(students);
        db.SaveChanges();
        foreach (var student in students)
            Console.WriteLine($"{student.FullName}: Id {student.Id}");
    }

    private static void EnrollStudent(LearningDbContext db)
    {
        PrintSection("Enroll student");
        var studentId = db.Students.Where(s => s.FullName == "Nour Ahmed")
            .OrderByDescending(s => s.Id).Select(s => s.Id).FirstOrDefault();
        int courseId = 1;
        if (!db.Students.Any(s => s.Id == studentId))
        {
            Console.WriteLine("Enrollment refused: student does not exist.");
            return;
        }
        var course = db.Courses.Find(courseId);
        if (course == null)
        {
            Console.WriteLine("Enrollment refused: course does not exist.");
            return;
        }
        if (!course.IsPublished)
        {
            Console.WriteLine("Enrollment refused: course is unpublished.");
            return;
        }
        if (db.Enrollments.Any(e => e.StudentId == studentId && e.CourseId == courseId))
        {
            Console.WriteLine("Enrollment refused: student is already enrolled.");
            return;
        }
        db.Enrollments.Add(new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow,
            ProgressPercentage = 0,
            Status = EnrollmentStatus.Active
        });
        db.SaveChanges();
        Console.WriteLine($"Enrolled student {studentId} in course {courseId}.");
    }

    private static void FindCourse(LearningDbContext db)
    {
        PrintSection("Find course");
        var byId = db.Courses.Find(1);
        Console.WriteLine(byId == null ? "Course by Id: not found" : $"By Id: {byId.Title}");
        var byTitle = db.Courses.FirstOrDefault(c => c.Title == "ASP.NET Core");
        Console.WriteLine(byTitle == null ? "Course by title: not found" : $"By title: {byTitle.Title}");
    }

    private static void FilterCourses(LearningDbContext db)
    {
        PrintSection("Filter courses");
        decimal minimum = 250m;
        decimal maximum = 700m;
        var courses = db.Courses.Where(c => c.Price >= minimum && c.Price <= maximum)
            .OrderByDescending(c => c.Price)
            .Select(c => new { c.Title, c.Level, c.Price }).ToList();
        foreach (var course in courses)
            Console.WriteLine($"{course.Title} | {course.Level} | {course.Price:C}");
        if (courses.Count == 0) Console.WriteLine("No matching courses.");
    }

    private static void PrintCategories(LearningDbContext db)
    {
        PrintSection("Sub-categories and top-level categories");
        int parentId = 1;
        var children = db.Categories.Where(c => c.ParentCategoryId == parentId).ToList();
        Console.WriteLine($"Direct children of category {parentId}:");
        foreach (var child in children) Console.WriteLine(child.Name);
        Console.WriteLine("Top-level categories:");
        foreach (var category in db.Categories.Where(c => c.ParentCategoryId == null).ToList())
            Console.WriteLine(category.Name);
    }

    private static void CourseRoster(LearningDbContext db)
    {
        PrintSection("Course roster");
        int courseId = 1;
        if (!db.Courses.Any(c => c.Id == courseId))
        {
            Console.WriteLine("Course not found.");
            return;
        }
        var studentIds = db.Enrollments.Where(e => e.CourseId == courseId)
            .Select(e => e.StudentId).ToList();
        var students = db.Students.Where(s => studentIds.Contains(s.Id)).ToList();
        Console.WriteLine($"Enrolled students: {studentIds.Count}");
        foreach (var student in students) Console.WriteLine(student.FullName);
    }

    private static void ChangePrice(LearningDbContext db)
    {
        PrintSection("Change course price");
        int courseId = 2;
        decimal newPrice = 650m;
        var course = db.Courses.Find(courseId);
        if (course == null)
        {
            Console.WriteLine("Course not found.");
            return;
        }
        if (newPrice < 0)
        {
            Console.WriteLine("Price cannot be negative.");
            return;
        }
        course.Price = newPrice;
        db.SaveChanges();
        Console.WriteLine($"Updated {course.Title} to {course.Price:C}.");
    }

    private static void UpdateProgress(LearningDbContext db)
    {
        PrintSection("Update enrollment progress");
        var studentId = db.Students.Where(s => s.Email == "adam@student.com")
            .Select(s => s.Id).FirstOrDefault();
        int courseId = 1;
        int newProgress = 100;
        if (newProgress is < 0 or > 100)
        {
            Console.WriteLine("Invalid progress: enter a value from 0 to 100.");
            return;
        }
        var enrollment = db.Enrollments.Find(studentId, courseId);
        if (enrollment == null)
        {
            Console.WriteLine("Enrollment not found.");
            return;
        }
        enrollment.ProgressPercentage = newProgress;
        if (newProgress == 100) enrollment.Status = EnrollmentStatus.Completed;
        else if (enrollment.Status == EnrollmentStatus.Completed)
        {
            enrollment.Status = EnrollmentStatus.Active;
            enrollment.FinalGrade = null;
        }
        db.SaveChanges();
        Console.WriteLine($"Progress: {enrollment.ProgressPercentage}%, status: {enrollment.Status}.");
    }

    private static void PublishCourse(LearningDbContext db)
    {
        PrintSection("Publish course");
        var course = db.Courses.Find(3);
        if (course == null)
        {
            Console.WriteLine("Course not found.");
            return;
        }
        if (course.IsPublished)
        {
            Console.WriteLine("Course already published.");
            return;
        }
        course.IsPublished = true;
        course.PublishedAt = DateTime.UtcNow;
        db.SaveChanges();
        Console.WriteLine($"Published {course.Title} at {course.PublishedAt:yyyy-MM-dd HH:mm}.");
    }

    private static void Unenroll(LearningDbContext db)
    {
        PrintSection("Unenroll student");
        var studentId = db.Students.Where(s => s.Email == "youssef@student.com")
            .Select(s => s.Id).FirstOrDefault();
        var enrollment = db.Enrollments.Find(studentId, 1);
        if (enrollment == null)
        {
            Console.WriteLine("Enrollment not found.");
            return;
        }
        db.Enrollments.Remove(enrollment);
        db.SaveChanges();
        Console.WriteLine("Enrollment removed.");
    }

    private static void DeleteCategoryWithException(LearningDbContext db)
    {
        PrintSection("Delete parent category and catch exception");
        var category = db.Categories.Find(1);
        if (category == null)
        {
            Console.WriteLine("Category not found.");
            return;
        }
        try
        {
            db.Categories.Remove(category);
            db.SaveChanges();
            Console.WriteLine("Category deleted.");
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"Delete refused: {ex.GetBaseException().Message}");
            db.Entry(category).State = EntityState.Detached;
        }
    }

    private static void DeleteCategorySafely(LearningDbContext db)
    {
        PrintSection("Safe category deletion");
        int categoryId = 2;
        var category = db.Categories.Find(categoryId);
        if (category == null)
        {
            Console.WriteLine("Category not found.");
            return;
        }
        if (db.Categories.Any(c => c.ParentCategoryId == categoryId))
        {
            Console.WriteLine("Cannot delete category because it has sub-categories.");
            return;
        }
        if (db.Courses.Any(c => c.CategoryId == categoryId))
        {
            Console.WriteLine("Cannot delete category because it still has courses.");
            return;
        }
        db.Categories.Remove(category);
        db.SaveChanges();
        Console.WriteLine("Category deleted safely.");
    }

    private static void DeleteCourse(LearningDbContext db)
    {
        PrintSection("Delete course with enrollments");
        int courseId = 4;
        var course = db.Courses.Find(courseId);
        if (course == null)
        {
            Console.WriteLine("Course not found.");
            return;
        }
        int before = db.Enrollments.Count(e => e.CourseId == courseId);
        Console.WriteLine($"Enrollments before deletion: {before}");
        db.Courses.Remove(course);
        db.SaveChanges();
        int after = db.Enrollments.Count(e => e.CourseId == courseId);
        Console.WriteLine($"Enrollments after deletion: {after}");
        Console.WriteLine("Enrollments were removed automatically because the Course foreign key uses cascade deletion.");
    }

    private static void BulkPriceRaise(LearningDbContext db)
    {
        PrintSection("Bulk price raise");
        CourseLevel level = CourseLevel.Beginner;
        int affected = db.Courses.Where(c => c.IsPublished && c.Level == level)
            .ExecuteUpdate(setters => setters.SetProperty(c => c.Price, c => c.Price * 1.10m));
        Console.WriteLine($"Updated {affected} course prices by 10%.");
    }

    private static void BulkDeleteDropped(LearningDbContext db)
    {
        PrintSection("Bulk delete dropped enrollments");
        int affected = db.Enrollments.Where(e => e.Status == EnrollmentStatus.Dropped).ExecuteDelete();
        Console.WriteLine($"Deleted {affected} dropped enrollments.");
    }
}
