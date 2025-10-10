import { Organization, OrgType } from "@/services/organizationService";

export interface OrganizationLevel {
  orgTypeUID: string;
  orgTypeName: string;
  organizations: Organization[];
  selectedOrgUID?: string;
  level: number;
  parentOrgUID?: string;
  dynamicLabel?: string;
}

export interface OrganizationHierarchyState {
  orgLevels: OrganizationLevel[];
  selectedOrgs: string[];
}

export function buildOrganizationTree(organizations: Organization[]): any[] {
  const orgMap = new Map<string, any>();
  const rootNodes: any[] = [];
  
  organizations.forEach(org => {
    orgMap.set(org.UID, {
      ...org,
      children: []
    });
  });
  
  organizations.forEach(org => {
    const node = orgMap.get(org.UID);
    if (org.ParentUID && orgMap.has(org.ParentUID)) {
      const parent = orgMap.get(org.ParentUID);
      parent.children.push(node);
    } else {
      rootNodes.push(node);
    }
  });
  
  return rootNodes;
}

function generateDynamicLabel(
  typeName: string
): string {
  return typeName;
}

export function initializeOrganizationHierarchy(
  organizations: Organization[],
  orgTypes: OrgType[]
): OrganizationLevel[] {
  const rootOrgs = organizations.filter(org => !org.ParentUID || org.ParentUID === '');
  
  const tree = buildOrganizationTree(organizations);
  
  if (rootOrgs.length === 0) {
    const orgsByType = new Map<string, Organization[]>();
    organizations.forEach(org => {
      if (!orgsByType.has(org.OrgTypeUID)) {
        orgsByType.set(org.OrgTypeUID, []);
      }
      orgsByType.get(org.OrgTypeUID)!.push(org);
    });
    
    const initialLevels: OrganizationLevel[] = [];
    let levelIndex = 0;
    orgsByType.forEach((orgs, typeUID) => {
      const type = orgTypes.find(t => t.UID === typeUID);
      if (type) {
        initialLevels.push({
          orgTypeUID: typeUID,
          orgTypeName: type.Name,
          organizations: orgs,
          level: levelIndex,
          dynamicLabel: generateDynamicLabel(type.Name)
        });
        levelIndex++;
      }
    });
    return initialLevels;
  }
  
  const rootOrgsByType = new Map<string, Organization[]>();
  rootOrgs.forEach(org => {
    if (!rootOrgsByType.has(org.OrgTypeUID)) {
      rootOrgsByType.set(org.OrgTypeUID, []);
    }
    rootOrgsByType.get(org.OrgTypeUID)!.push(org);
  });
  
  const initialLevels: OrganizationLevel[] = [];
  let levelIndex = 0;
  
  rootOrgsByType.forEach((orgs, typeUID) => {
    const type = orgTypes.find(t => t.UID === typeUID);
    
    if (type) {
      const dynamicLabel = generateDynamicLabel(type.Name);
      initialLevels.push({
        orgTypeUID: typeUID,
        orgTypeName: type.Name,
        organizations: orgs,
        level: 0,
        dynamicLabel
      });
    } else {
      const dynamicLabel = generateDynamicLabel(typeUID);
      initialLevels.push({
        orgTypeUID: typeUID,
        orgTypeName: typeUID,
        organizations: orgs,
        level: 0,
        dynamicLabel
      });
    }
    levelIndex++;
  });
  return initialLevels;
}

export function handleOrganizationSelection(
  levelIndex: number,
  selectedValue: string,
  currentLevels: OrganizationLevel[],
  currentSelectedOrgs: string[],
  organizations: Organization[],
  orgTypes: OrgType[]
): {
  updatedLevels: OrganizationLevel[];
  updatedSelectedOrgs: string[];
} {
  if (!selectedValue) {
    return {
      updatedLevels: currentLevels,
      updatedSelectedOrgs: currentSelectedOrgs
    };
  }
  
  const updatedSelectedOrgs = [...currentSelectedOrgs.slice(0, levelIndex), selectedValue];
  
  const updatedLevels = [...currentLevels.slice(0, levelIndex + 1)];
  updatedLevels[levelIndex] = {
    ...updatedLevels[levelIndex],
    selectedOrgUID: selectedValue
  };
  
  const selectedOrg = organizations.find(org => org.UID === selectedValue);
  
  if (!selectedOrg) {
    return {
      updatedLevels,
      updatedSelectedOrgs
    };
  }
  
  const childOrgs = organizations.filter(org => org.ParentUID === selectedValue);
  
  if (childOrgs.length > 0) {
    const childOrgsByType = new Map<string, Organization[]>();
    
    for (const childOrg of childOrgs) {
      if (!childOrgsByType.has(childOrg.OrgTypeUID)) {
        childOrgsByType.set(childOrg.OrgTypeUID, []);
      }
      childOrgsByType.get(childOrg.OrgTypeUID)!.push(childOrg);
    }
    
    const nextLevel = levelIndex + 1;
    childOrgsByType.forEach((orgs, typeUID) => {
      const childType = orgTypes.find(t => t.UID === typeUID);
      if (orgs.length > 0) {
        const typeName = childType?.Name || typeUID;
        const dynamicLabel = generateDynamicLabel(typeName);
        
        updatedLevels.push({
          orgTypeUID: typeUID,
          orgTypeName: typeName,
          organizations: orgs,
          level: nextLevel,
          parentOrgUID: selectedValue,
          dynamicLabel
        });
      }
    });
  }
  
  return {
    updatedLevels,
    updatedSelectedOrgs
  };
}

export function getFinalSelectedOrganization(selectedOrgs: string[]): string | undefined {
  return selectedOrgs.length > 0 ? selectedOrgs[selectedOrgs.length - 1] : undefined;
}

export function getOrganizationDisplayName(org: Organization): string {
  return org.Code ? `${org.Name} (${org.Code})` : org.Name;
}

export function resetOrganizationHierarchy(
  organizations: Organization[],
  orgTypes: OrgType[]
): {
  resetLevels: OrganizationLevel[];
  resetSelectedOrgs: string[];
} {
  const resetLevels = initializeOrganizationHierarchy(organizations, orgTypes);
  const resetSelectedOrgs: string[] = [];
  
  return {
    resetLevels,
    resetSelectedOrgs
  };
}

export function validateOrganizationSelection(selectedOrgs: string[]): boolean {
  return selectedOrgs.length > 0;
}