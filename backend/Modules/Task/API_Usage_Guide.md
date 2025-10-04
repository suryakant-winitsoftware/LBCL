# Task Management API Usage Guide

This guide provides examples of how to use the Task Management API endpoints that have been implemented.

## Base URL
```
https://your-api-domain/api/Task
```

## Authentication
All endpoints require JWT Bearer token authentication.

## Available Endpoints

### 1. Task CRUD Operations

#### Get All Tasks (Paginated)
```http
POST /api/Task/GetAllTasks
Content-Type: application/json
Authorization: Bearer {your-jwt-token}

{
  "pageNumber": 1,
  "pageSize": 10,
  "isCountRequired": true,
  "sortCriterias": [],
  "filterCriterias": []
}
```

#### Get Task by UID
```http
GET /api/Task/GetTaskByUID/{uid}
Authorization: Bearer {your-jwt-token}
```

#### Create New Task
```http
POST /api/Task/CreateTask
Content-Type: application/json
Authorization: Bearer {your-jwt-token}

{
  "title": "Store Compliance Survey",
  "description": "Conduct monthly compliance survey for all stores",
  "taskTypeId": 1,
  "taskSubTypeId": 1,
  "salesOrgId": 100,
  "startDate": "2025-09-24T00:00:00Z",
  "endDate": "2025-09-30T23:59:59Z",
  "isActive": true,
  "priority": "High",
  "taskData": "{\"checklist\": [\"Safety compliance\", \"Inventory accuracy\"]}"
}
```

#### Update Task
```http
PUT /api/Task/UpdateTask
Content-Type: application/json
Authorization: Bearer {your-jwt-token}

{
  "id": 1,
  "uid": "task-uid-here",
  "title": "Updated Task Title",
  "description": "Updated description",
  "taskTypeId": 1,
  "startDate": "2025-09-24T00:00:00Z",
  "endDate": "2025-09-30T23:59:59Z",
  "isActive": true,
  "priority": "Medium",
  "status": "Active"
}
```

#### Delete Task
```http
DELETE /api/Task/DeleteTask/{uid}
Authorization: Bearer {your-jwt-token}
```

### 2. Task Types Management

#### Get All Task Types
```http
GET /api/Task/GetAllTaskTypes
Authorization: Bearer {your-jwt-token}
```

#### Get Task Sub Types by Task Type
```http
GET /api/Task/GetTaskSubTypesByTaskType/{taskTypeId}
Authorization: Bearer {your-jwt-token}
```

### 3. Task Assignment Operations

#### Assign Task to Users/Groups
```http
POST /api/Task/AssignTask
Content-Type: application/json
Authorization: Bearer {your-jwt-token}

{
  "taskId": 1,
  "assignedToType": "User",
  "userIds": [101, 102, 103],
  "userGroupIds": [],
  "notes": "Please complete by end of week"
}
```

#### Get User's Task Assignments
```http
GET /api/Task/GetUserTaskAssignments/{userId}
Authorization: Bearer {your-jwt-token}
```

#### Update Task Assignment Progress
```http
PUT /api/Task/UpdateTaskAssignment
Content-Type: application/json
Authorization: Bearer {your-jwt-token}

{
  "uid": "assignment-uid-here",
  "status": "InProgress",
  "progress": 50,
  "notes": "Half way completed"
}
```

### 4. Filtering and Search

#### Filter Tasks
```http
POST /api/Task/GetTasksByFilter
Content-Type: application/json
Authorization: Bearer {your-jwt-token}

{
  "taskTypeId": 1,
  "taskSubTypeId": 1,
  "salesOrgId": 100,
  "status": "Active",
  "startDate": "2025-09-01T00:00:00Z",
  "endDate": "2025-09-30T23:59:59Z",
  "assignedUserId": 101,
  "isActive": true,
  "pageNumber": 1,
  "pageSize": 20
}
```

### 5. Dashboard APIs

#### Get Tasks Dashboard
```http
GET /api/Task/GetTasksDashboard?userId=101&salesOrgId=100
Authorization: Bearer {your-jwt-token}
```

#### Get Task Status Counts
```http
GET /api/Task/GetTaskStatusCounts?userId=101&salesOrgId=100
Authorization: Bearer {your-jwt-token}
```

### 6. Utility Endpoints

#### Validate Task Dates
```http
POST /api/Task/ValidateTaskDates?startDate=2025-09-24&endDate=2025-09-30
Authorization: Bearer {your-jwt-token}
```

#### Check User Access to Task
```http
GET /api/Task/CanUserAccessTask/{taskId}/{userId}
Authorization: Bearer {your-jwt-token}
```

## Response Format

### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    // Response data here
  },
  "errors": null
}
```

### Error Response
```json
{
  "success": false,
  "message": "An error occurred while processing the request",
  "data": null,
  "errors": [
    "Detailed error message"
  ]
}
```

## Status Codes

- `200 OK` - Request successful
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Access denied
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

## Task Status Values

- `Draft` - Task created but not yet active
- `Active` - Task is currently active and can be worked on
- `Completed` - Task has been completed
- `Cancelled` - Task has been cancelled
- `OnHold` - Task is temporarily on hold

## Assignment Status Values

- `Pending` - Assignment created but not yet accepted
- `Assigned` - Assignment accepted by user
- `InProgress` - User is actively working on the task
- `Completed` - Task assignment completed
- `Cancelled` - Assignment cancelled
- `Overdue` - Assignment is past due date

## Priority Values

- `Low` - Low priority task
- `Medium` - Medium priority task (default)
- `High` - High priority task
- `Critical` - Critical priority task

## Integration with Frontend

These APIs support the frontend forms shown in your screenshots:

### For "Add/Edit Task" Form:
1. Use `GetAllTaskTypes` to populate the Task Type dropdown
2. Use `GetTaskSubTypesByTaskType` to populate Task Sub Type dropdown
3. Use `CreateTask` or `UpdateTask` to save the form data

### For "Assign Task" Page:
1. Use `GetTaskByUID` to display task details
2. Use `AssignTask` to assign to selected users/groups
3. Use `GetTaskAssignments` to show current assignments

### For Dashboard/List Views:
1. Use `GetTasksByFilter` for filtered lists
2. Use `GetTasksDashboard` for user-specific task views
3. Use `GetTaskStatusCounts` for status summary widgets

## Next Steps

1. **Database Setup**: Run the `task_schema.sql` file to create the required tables
2. **Build & Test**: Build the project and test the API endpoints
3. **Frontend Integration**: Integrate these APIs with your existing frontend
4. **User Authorization**: Implement proper user context in the BL layer
5. **Notifications**: Consider adding notifications for task assignments and updates