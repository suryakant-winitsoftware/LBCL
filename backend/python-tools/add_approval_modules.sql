-- Add Approval Management Sub-Module
INSERT INTO public.sub_modules (
    uid,
    submodule_name_en,
    submodule_name_other,
    relative_path,
    serial_no,
    module_uid,
    show_in_menu,
    is_for_distributor,
    is_for_principal,
    ss,
    created_by,
    modified_by,
    created_time,
    modified_time,
    server_add_time,
    server_modified_time,
    check_in_permission
) VALUES (
    'SystemAdministration_ApprovalManagement',
    'Approval Management',
    'Approval Management',
    'approvals',
    27,  -- Next serial number after Currency (26)
    'SystemAdministration',
    true,
    true,
    true,
    0,
    'ADMIN',
    'ADMIN',
    NOW(),
    NOW(),
    NOW(),
    NOW(),
    null
);

-- Add Approval Dashboard Sub-Sub-Module
INSERT INTO public.sub_sub_modules (
    uid,
    sub_sub_module_name_en,
    sub_sub_module_name_other,
    relative_path,
    serial_no,
    sub_module_uid,
    show_in_menu,
    is_for_distributor,
    is_for_principal,
    ss,
    created_by,
    modified_by,
    created_time,
    modified_time,
    server_add_time,
    server_modified_time,
    check_in_permission
) VALUES (
    'SystemAdministration_ApprovalManagement_Dashboard',
    'Approval Dashboard',
    'Approval Dashboard',
    'approvals',
    1,
    'SystemAdministration_ApprovalManagement',
    true,
    true,
    true,
    0,
    'ADMIN',
    'ADMIN',
    NOW(),
    NOW(),
    NOW(),
    NOW(),
    null
);

-- Add Pending Approvals Sub-Sub-Module
INSERT INTO public.sub_sub_modules (
    uid,
    sub_sub_module_name_en,
    sub_sub_module_name_other,
    relative_path,
    serial_no,
    sub_module_uid,
    show_in_menu,
    is_for_distributor,
    is_for_principal,
    ss,
    created_by,
    modified_by,
    created_time,
    modified_time,
    server_add_time,
    server_modified_time,
    check_in_permission
) VALUES (
    'SystemAdministration_ApprovalManagement_Pending',
    'Pending Approvals',
    'Pending Approvals',
    'approvals?tab=pending',
    2,
    'SystemAdministration_ApprovalManagement',
    true,
    true,
    true,
    0,
    'ADMIN',
    'ADMIN',
    NOW(),
    NOW(),
    NOW(),
    NOW(),
    null
);

-- Add Approved Requests Sub-Sub-Module
INSERT INTO public.sub_sub_modules (
    uid,
    sub_sub_module_name_en,
    sub_sub_module_name_other,
    relative_path,
    serial_no,
    sub_module_uid,
    show_in_menu,
    is_for_distributor,
    is_for_principal,
    ss,
    created_by,
    modified_by,
    created_time,
    modified_time,
    server_add_time,
    server_modified_time,
    check_in_permission
) VALUES (
    'SystemAdministration_ApprovalManagement_Approved',
    'Approved Requests',
    'Approved Requests',
    'approvals?tab=approved',
    3,
    'SystemAdministration_ApprovalManagement',
    true,
    true,
    true,
    0,
    'ADMIN',
    'ADMIN',
    NOW(),
    NOW(),
    NOW(),
    NOW(),
    null
);

-- Add Rejected Requests Sub-Sub-Module
INSERT INTO public.sub_sub_modules (
    uid,
    sub_sub_module_name_en,
    sub_sub_module_name_other,
    relative_path,
    serial_no,
    sub_module_uid,
    show_in_menu,
    is_for_distributor,
    is_for_principal,
    ss,
    created_by,
    modified_by,
    created_time,
    modified_time,
    server_add_time,
    server_modified_time,
    check_in_permission
) VALUES (
    'SystemAdministration_ApprovalManagement_Rejected',
    'Rejected Requests',
    'Rejected Requests',
    'approvals?tab=rejected',
    4,
    'SystemAdministration_ApprovalManagement',
    true,
    true,
    true,
    0,
    'ADMIN',
    'ADMIN',
    NOW(),
    NOW(),
    NOW(),
    NOW(),
    null
);

-- Add Approval History Sub-Sub-Module
INSERT INTO public.sub_sub_modules (
    uid,
    sub_sub_module_name_en,
    sub_sub_module_name_other,
    relative_path,
    serial_no,
    sub_module_uid,
    show_in_menu,
    is_for_distributor,
    is_for_principal,
    ss,
    created_by,
    modified_by,
    created_time,
    modified_time,
    server_add_time,
    server_modified_time,
    check_in_permission
) VALUES (
    'SystemAdministration_ApprovalManagement_History',
    'Approval History',
    'Approval History',
    'approvals?tab=all',
    5,
    'SystemAdministration_ApprovalManagement',
    true,
    true,
    true,
    0,
    'ADMIN',
    'ADMIN',
    NOW(),
    NOW(),
    NOW(),
    NOW(),
    null
);

-- Verify insertions
SELECT 'Sub-Module Created:' as message, * FROM public.sub_modules WHERE uid = 'SystemAdministration_ApprovalManagement';

SELECT 'Sub-Sub-Modules Created:' as message, * FROM public.sub_sub_modules WHERE sub_module_uid = 'SystemAdministration_ApprovalManagement' ORDER BY serial_no;
