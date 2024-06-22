export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  birthDate: Date;
  phoneNumber: string;
  photo: string;
  role: string | null;
}
