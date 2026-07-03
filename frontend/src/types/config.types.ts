// System config types — mirrors PRM.Application.DTOs.SystemConfig

export interface SystemConfigResponse {
  key: string;
  value: string;
}

export interface UpdateSystemConfigRequest {
  value: string;
}
