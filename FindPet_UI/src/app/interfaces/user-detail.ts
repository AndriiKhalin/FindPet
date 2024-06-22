export interface UserDetail {
  id: string;
  name: string;
  email: string;
  photo: string;
  birthDate:Date;
  role: string[];
  phoneNumber: string;
  twoFacotrEnabled: boolean;
  phoneNumberConfirmed: boolean;
  accessFailedCount: 0;
}
